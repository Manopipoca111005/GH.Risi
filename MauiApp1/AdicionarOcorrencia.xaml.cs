using CommunityToolkit.Maui.Core.Views;
using GH_Metodos;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace MauiApp1;

public partial class AdicionarOcorrencia : ContentPage
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    private readonly int IdColaborador;
    private readonly string Token;
    private readonly string nome_abreviado;
    public const string OpcaoDefinicoes = "Definições";
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";


    public ObservableCollection<AssiduidadeModel> Lista { get; set; }


    public AdicionarOcorrencia(ObservableCollection<AssiduidadeModel> lista, int idColaborador, string token, string date,string nomeabreviado)
    {
        InitializeComponent();

        IdColaborador = idColaborador;
        Token = token;
        nome_abreviado = nomeabreviado;
        var dataFormatada = date;
        lbldata.Text = dataFormatada;
        this.Appearing += async (_, __) => await InicializarAsync();
        this.Appearing += async (_, __) => await InicializarServicosAsync();
        InicializarCamposEscondidos();
    }

    private async Task<List<Ocorrencia>> CarregarOcorrenciasAsync()
    {
        try
        {

            var resposta = await _service.GetOcorrenciasAsync(IdColaborador, Token);
            var ocorrencias = resposta.Body.GetOcorrenciasResult?.aOcorrencias?.ToList();
            return ocorrencias ?? new List<Ocorrencia>();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Falha ao carregar ocorrências: " + ex.Message, "OK");
            return new List<Ocorrencia>();
        }
    }

    private async Task InicializarAsync()
    {
        var ocorrencias = await CarregarOcorrenciasAsync();
        PickerOcorrencias.ItemsSource = ocorrencias;
        PickerOcorrencias.DisplayMemberPath = "descOcorrencia";
    }


    private async Task<List<Servico>> CarregarServicosAsync()
    {
        try
        {

            var resposta = await _service.GetServicosAsync(IdColaborador, Token);
            var servicos = resposta.Body.GetServicosResult?.aServicos?.ToList();
            return servicos ?? new List<Servico>();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Falha ao carregar ocorrências: " + ex.Message, "OK");
            return new List<Servico>();
        }
    }

    private async Task InicializarServicosAsync()
    {
        var servicos = await CarregarServicosAsync();
        PickerServicos.ItemsSource = servicos;
        PickerServicos.DisplayMemberPath = "descServico";
    }

    private async Task EnviarOcorrenciaAsync(int idColaborador, string token, string data, short idOcorrencia, string idPMT, string horaInicio, string horaFim, string obsColaborador)
    {

        try
        {

            var resposta = await _service.SetOcorrenciaAccaoDiaAsync(idColaborador, token, data, idOcorrencia, idPMT, horaInicio, horaFim, obsColaborador);
            var result = resposta.Body.SetOcorrenciaAccaoDiaResult;

            if (result.erro != 0)
            {
                await DisplayAlert("Erro ao enviar", result.erroMensagem, "OK");
                return;
            }
            await DisplayAlert("Sucesso", "Ocorrencia enviada com sucesso!", "OK");
            await Navigation.PopAsync();

        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao enviar: {ex.Message}", "OK");
        }
    }


    private async void BtnGuardar_Clicked(object sender, EventArgs e)
    {
        if (PickerOcorrencias.SelectedItem is not Ocorrencia ocorrencias || PickerServicos.SelectedItem is not Servico servicos)
        {
            await DisplayAlert("Erro", "Selecione uma Ocorrencia válida + Selecione um Servico válido", "OK");
            return;
        }

        string obs = ocorrencias.obs.ToString();
        string data = lbldata.Text;
        string idpmt = servicos.idPMT.ToString();
        short idOcorrencia = ocorrencias.idOcorrencia;

        string horaInicio = hora.Time.ToString(@"hh\:mm");
        string horaFim = horafim.Time.ToString(@"hh\:mm");


        await EnviarOcorrenciaAsync(IdColaborador, Token, data, idOcorrencia, idpmt, horaInicio, horaFim, obs);

    }

    private void InicializarCamposEscondidos()
    {

        lblServico.IsVisible = false;
        FrameServico.IsVisible = false;
        PickerServicos.IsVisible = false;

        FrameHoraInicio.IsVisible = false;
        FrameHorafim.IsVisible = false;

        hora.IsVisible = false;
        horafim.IsVisible = false;

        lblHorafim.IsVisible = false;
        lblHorainicio.IsVisible = false;




    }
    private void PickerOcorrencias_SelectionChanged(object sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e) // Ajustado o tipo de EventArgs se estiver a usar SfComboBox
    {


        if (PickerOcorrencias.SelectedItem is Ocorrencia ocorrencias)
        {
            AtualizarVisibilidadeCamposHora(ocorrencias.registaHoraInicio, ocorrencias.registaHoraFim, ocorrencias.registaServico);
        }
        else
        {

            InicializarCamposEscondidos();
        }
    }

    private void AtualizarVisibilidadeCamposHora(bool registaHoraInicio, bool registaHoraFim, bool registaServico)
    {
        bool mostrarHoras = registaHoraInicio;
        bool mostrarHorasFim = registaHoraFim;
        bool mostrarregisto = registaServico;
        


        lblServico.IsVisible = mostrarregisto;
        FrameServico.IsVisible = mostrarregisto; 
        PickerServicos.IsVisible = mostrarregisto;

        FrameHoraInicio.IsVisible = mostrarHoras;
        FrameHorafim.IsVisible = mostrarHorasFim;  

        hora.IsVisible = mostrarHoras;
        horafim.IsVisible = mostrarHorasFim;

        lblHorafim.IsVisible = mostrarHorasFim;
        lblHorainicio.IsVisible = mostrarHoras;


        if (mostrarHoras == true && mostrarHorasFim == false)
        {
            lblHorainicio.Text = "Hora";
        };

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
                    break;
            }
        }
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new AdicionarNotificacao(IdColaborador, Token, nome_abreviado));
    }
}