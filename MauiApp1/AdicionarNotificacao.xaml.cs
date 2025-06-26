using GH_Metodos;
namespace MauiApp1;

public partial class AdicionarNotificacao : ContentPage
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    private readonly int idColaborador;
    private readonly string Token;
    private readonly bool novas = false;
    private readonly string nome_abreviado;
    public const string OpcaoDefinicoes = "Definições";
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";
    public AdicionarNotificacao(int id_colaborador, string token, string NomeAbreviado)
	{
		InitializeComponent();
        idColaborador = id_colaborador;
        Token = token;
        nome_abreviado = NomeAbreviado;
        this.Appearing += async (_, __) => await InicializarAsync();
        string data = DateTime.Now.ToString("dd/MM/yyyy");
        Mensagem.Text = $"Notifica o(a) {NomeAbreviado} ao dia {data} a seguinte situação:";

    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        string acao = await DisplayActionSheet(
        "Menu de Opções",
        OpcaoCancelar,
        null,
        OpcaoDefinicoes,
        OpcaoSair
        );

        if (!string.IsNullOrEmpty(acao))
        {
            switch (acao)
            {
                case OpcaoDefinicoes:
                    await Navigation.PushAsync(new SettingsPage());
                    break;

                case OpcaoSair:
                    await Navigation.PushAsync(new MainPage());
                    break;

                case OpcaoCancelar:
                    await DisplayAlert("Ação", "Operação cancelada.", "OK");
                    break;
            }


        }

    }


    

    private async Task<List<DestinatarioNotificacao>> CarregarDestinatariosAsync()
    {
        try
        {
            var resposta = await _service.GetDestinatariosNotificacoesAsync(idColaborador, Token);
            var destinatarios = resposta.Body.GetDestinatariosNotificacoesResult?.aDestinatariosNotificacoes?.ToList();
            return destinatarios ?? new List<DestinatarioNotificacao>();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Falha ao carregar destinatários: " + ex.Message, "OK");
            return new List<DestinatarioNotificacao>();
        }
    }

    private async Task InicializarAsync()
    {
        var destinatarios = await CarregarDestinatariosAsync();
        PickerDestinatarios.ItemsSource = destinatarios;
        PickerDestinatarios.DisplayMemberPath = "descColaborador";


    }




    private async Task EnviarNotificacaoAsync(int idColaborador, string token, int numMecDestinatario, string assunto, string mensagem)
    {

        try
        {

            var resposta = await _service.SetNotificacaoAsync(idColaborador, token, numMecDestinatario, assunto, mensagem);
            var result = resposta.Body.SetNotificacaoResult;

            if (result.erro != 0)
            {
                await DisplayAlert("Erro ao enviar", result.erroMensagem, "OK");
                return;
            }
            await DisplayAlert("Sucesso", "Notificação enviada com sucesso!", "OK");
            await Navigation.PopAsync();
            
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao enviar: {ex.Message}", "OK");
        }
    }


    private async void BtnEnviarNotificacao_Clicked(object sender, EventArgs e)
    {
        if (PickerDestinatarios.SelectedItem is not DestinatarioNotificacao destinatario)
        {
            await DisplayAlert("Erro", "Selecione um destinatário válido.", "OK");
            return;
        }

        string assunto = EntryAssunto.Text;
        string mensagem = EntryMensagem.Text;

        if (string.IsNullOrWhiteSpace(assunto) || string.IsNullOrWhiteSpace(mensagem))
        {
            await DisplayAlert("Erro", "Assunto e mensagem são obrigatórios.", "OK");
            return;
        }

        await EnviarNotificacaoAsync(idColaborador, Token, destinatario.numMec, assunto, mensagem);
    }

   
}