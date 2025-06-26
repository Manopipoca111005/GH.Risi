namespace MauiApp1;
using GH_Metodos;
using System.Collections.ObjectModel;
using System.Globalization;

public partial class AdicionarTrocaPasso2 : ContentPage
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    public const string OpcaoDefinicoes = "Definições";
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";
    public readonly int IdColaborador;
    public readonly string Token;
    public  int Idpmt;
    public string nomeabreviado;
    public string Servico;
    public ObservableCollection<ServiceDisplayItem> ServicosDisponiveis { get; set; }
    public AdicionarTrocaPasso2(int idcolaborador, string token,int idpmt,string nome_abreviado, string servico)
	{
		InitializeComponent();
        IdColaborador = idcolaborador;
        Token = token;
        Idpmt = idpmt;
        nomeabreviado = nome_abreviado;
        Servico = servico;

        ServicosDisponiveis = new ObservableCollection<ServiceDisplayItem>();
        BindingContext = this;
        _ = LoadServicosAsync();

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
    private async Task LoadServicosAsync()
    {
        try
        {

            
            var resposta = await _service.GetColaboradoresAsync(Idpmt,IdColaborador, Token);
            var pmts = resposta?.Body?.GetColaboradoresResult?.aColaboradoresPMT;

            if (pmts != null && pmts.Length > 0)
            {
                CultureInfo culturaPtPt = new CultureInfo("pt-PT");
                int anoAtual = DateTime.Now.Year;
                int mesAtual = DateTime.Now.Month;

                ServicosDisponiveis.Clear();

                foreach (var item in pmts)
                {


                    ServicosDisponiveis.Add(new ServiceDisplayItem
                    {
                        ServiceName =  item.nomeAbreviado,
                        idcolab = item.idColaborador,


                    });
                    
                }

                if (ServicosDisponiveis.Count == 0)
                {
                    await DisplayAlert("Informação", "Nenhum serviço encontrado para o mês atual ou futuro.", "OK");
                }
            }
            else
            {
                ServicosDisponiveis.Clear();
                await DisplayAlert("Informação", "Nenhum serviço encontrado.", "OK");
            }

            if (resposta?.Body?.GetColaboradoresResult?._erro != null && resposta.Body.GetColaboradoresResult._erro.erro != 0)
            {
                await DisplayAlert("Erro do Serviço", resposta.Body.GetColaboradoresResult._erro.erroMensagem, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Ocorreu um erro ao carregar os serviços: " + ex.Message, "OK");
        }
    }

    public class ServiceDisplayItem
    {
        public string ServiceName { get; set; }
        
        public int idcolab { get; set; }

        public int nomecolabtroca { get; set; }

    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (serviceComboBox.SelectedItem is not ServiceDisplayItem selecionado)
        {
            await DisplayAlert("Aviso", "Por favor, selecione um serviço.", "OK");
            return;
        }

        if (serviceComboBox.SelectedItem != null)
        {
            await Navigation.PushAsync(new AdicionarTrocaPasso3(IdColaborador, Token, Idpmt,selecionado.idcolab,nomeabreviado,selecionado.ServiceName,Servico));
        }
        else
        {
            await DisplayAlert(null, "Selecione um serviço para avancar ", "OK");
        }
        
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AdicionarTroca(IdColaborador, Token, Idpmt, nomeabreviado));
    }
}