using GH_Metodos;
using MauiApp1.Services; // Adicionado

namespace MauiApp1;

public partial class DadosPessoais : ContentPage
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    private readonly int idColaborador;
    private readonly string token;
    private readonly string nome_abreviado;
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";
    private readonly ICalendarService _calendarService; // Adicionado

    public DadosPessoais(int IdColaborador, string Token, string NomeAbreviado)
    {
        InitializeComponent();

        idColaborador = IdColaborador;
        token = Token;
        nome_abreviado = NomeAbreviado;
        User.Text = nome_abreviado;
        Id.Text = idColaborador.ToString();
        CarregarDadosPessoais();

        // Obtenha o ServiceProvider e resolva a instância de ICalendarService
        var serviceProvider = App.Current.Handler.MauiContext.Services;
        _calendarService = serviceProvider.GetService<ICalendarService>();
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        string acao = await DisplayActionSheet(
        "Menu de Opções",
        OpcaoCancelar,
        null,
        OpcaoSair
        );

        if (!string.IsNullOrEmpty(acao))
        {
            switch (acao)
            {
                case OpcaoSair:
                    await Navigation.PushAsync(new MainPage());
                    break;

                case OpcaoCancelar:
                    await DisplayAlert("Ação", "Operação cancelada.", "OK");
                    break;
            }
        }
    }

    private async Task CarregarDadosPessoais()
    {
        try
        {
            var resposta = await _service.GetDadosPessoaisAsync(idColaborador, token);
            var dados = resposta.Body.GetDadosPessoaisResult;

            if (dados != null)
            {
                EmailAlternativoEntry.Text = dados.emailAlternativo;
                TelefoneFixoEntry.Text = dados.telefoneFixo;
                Telemovel1Entry.Text = dados.telemovel1;
                Telemovel2Entry.Text = dados.telemovel2;
                ExtensaoEntry.Text = dados.extensao;
                TelemovelProfissionalEntry.Text = dados.telemovelProfissional;
            }
            else
            {
                await DisplayAlert("Erro", "Dados pessoais não encontrados.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar dados pessoais: {ex.Message}", "OK");
        }
    }

    private async void OnGravarClicked(object sender, EventArgs e)
    {
        try
        {
            var resposta = await _service.SetDadosPessoaisAsync(
                idColaborador,
                token,
                EmailAlternativoEntry.Text,
                TelefoneFixoEntry.Text,
                Telemovel1Entry.Text,
                Telemovel2Entry.Text,
                ExtensaoEntry.Text,
                TelemovelProfissionalEntry.Text
            );

            var resultado = resposta.Body.SetDadosPessoaisResult;

            if (resultado.erro == 0)
            {
                await DisplayAlert("Sucesso", "Dados gravados com sucesso!", "OK");

                // Verificação adicionada para o caso de o serviço não estar disponível (ex: em outras plataformas)
                if (_calendarService == null)
                {
                    await DisplayAlert("Erro", "Serviço de calendário não disponível. Contacte o suporte.", "OK");
                    return;
                }

                // Linha modificada para passar o calendarService
                await Navigation.PushAsync(new OptionsPage(idColaborador, token, nome_abreviado, _calendarService));
            }
            else
            {
                await DisplayAlert("Erro", $"Erro ao gravar: {resultado.erroMensagem}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Falha ao gravar dados: " + ex.Message, "OK");
        }
    }
}