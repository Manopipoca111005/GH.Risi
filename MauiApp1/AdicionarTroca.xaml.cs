using Syncfusion.Maui.Inputs;
using System.Collections.ObjectModel;
using System.Globalization;
using GH_Metodos;
using System.Threading.Tasks;
namespace MauiApp1;

public partial class AdicionarTroca : ContentPage
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    public const string OpcaoDefinicoes = "Definições";
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";
    public readonly int IdColaborador;
    public readonly string Token;
    public readonly string nomeabreviado;
    public int Idpmt;    


    public ObservableCollection<ServiceDisplayItem> ServicosDisponiveis { get; set; }

    public AdicionarTroca(int idcolaborador, string token,int idpmt ,string nome_abreviado)
    {
        InitializeComponent();
        IdColaborador = idcolaborador;
        Token = token;
        nomeabreviado = nome_abreviado;
        Idpmt = idpmt;
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
            var resposta = await _service.GetServicosAsync(IdColaborador, Token);
            var pmts = resposta?.Body?.GetServicosResult?.aServicos;

            if (pmts != null && pmts.Length > 0)
            {
                CultureInfo culturaPtPt = new CultureInfo("pt-PT");
                int anoAtual = DateTime.Now.Year;
                int mesAtual = DateTime.Now.Month;

                ServicosDisponiveis.Clear();

                foreach (var item in pmts)
                {
                    if (item.ano > anoAtual || (item.ano == anoAtual && item.mes >= mesAtual))
                    {
                        DateTimeFormatInfo dtfi = culturaPtPt.DateTimeFormat;
                        string nomeDoMes = dtfi.GetMonthName(item.mes);

                        ServicosDisponiveis.Add(new ServiceDisplayItem
                        {
                            ServiceName = item.descServico,
                            idpmt = item.idPMT,
                            FormattedDate = $"{nomeDoMes} de {item.ano}"

                        });
                    }
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

            if (resposta?.Body?.GetServicosResult?._erro != null && resposta.Body.GetServicosResult._erro.erro != 0)
            {
                await DisplayAlert("Erro do Serviço", resposta.Body.GetServicosResult._erro.erroMensagem, "OK");
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
        public string FormattedDate { get; set; }

        public int idpmt { get; set; }

    }

 

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        if (serviceComboBox.SelectedItem is not ServiceDisplayItem selecionado)
        {
            await DisplayAlert("Aviso", "Por favor, selecione um serviço.", "OK");
            return;
        }

        if (serviceComboBox.SelectedItem != null) {
            
            await Navigation.PushAsync(new AdicionarTrocaPasso2(IdColaborador, Token,selecionado.idpmt,nomeabreviado,selecionado.ServiceName));
            
        }
        else
        {
            await DisplayAlert(null, "Selecione um serviço para avancar ", "OK");
        }
    }
}
