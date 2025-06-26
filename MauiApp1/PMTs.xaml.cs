using GH_Metodos;
namespace MauiApp1;

using Microsoft.Maui.Controls;
using System.Globalization;

public partial class PMTs : ContentPage
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    public const string OpcaoDefinicoes = "Definições";
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";
    private readonly int idColaborador;
    private readonly string Token;
    private readonly string nome_abreviado;

    public PMTs(int id_colaborador, string token, string NomeAbreviado)
    {
        InitializeComponent();
        idColaborador = id_colaborador;
        Token = token;
        nome_abreviado = NomeAbreviado;
        CarregarNotificacoesAsync();
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

    private async Task CarregarNotificacoesAsync()
    {
        try
        {
            
var resposta = await _service.GetServicosAsync(idColaborador, Token);
            var pmts = resposta?.Body?.GetServicosResult?.aServicos;
            StackPMTs.Children.Clear();

            if (pmts != null && pmts.Length > 0)
            {
                CultureInfo cultura = new CultureInfo("pt-PT");

                foreach (var item in pmts) // 'item' representa cada pmt
                {
                    string nomeMes = cultura.DateTimeFormat.GetMonthName(item.mes);
                    string mesAnoFormatado = $"{nomeMes} de {item.ano}";

                    var textLayout = new VerticalStackLayout
                    {
                        Spacing = 2,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                    {
                        new Label { Text = item.descServico, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("Black")},
                        new Label { Text = mesAnoFormatado, TextColor = Color.FromArgb("#007BA7")}
                    }
                    };

                    var imageAndTextLayout = new HorizontalStackLayout
                    {
                        Spacing = 8,
                        Children =
                    {
                        new Image
                        {
                            Source = "calend.png",
                            HeightRequest = 40,
                            WidthRequest = 40,
                            Aspect = Aspect.AspectFit,
                            VerticalOptions = LayoutOptions.Center
                        },
                        textLayout
                    }
                    };


                    var entryItemLayout = new VerticalStackLayout
                    {
                        Spacing = 4,
                        Padding = new Thickness(0, 2),
                        Children =
                    {
                        imageAndTextLayout,
                        new BoxView
                        {
                            HeightRequest = 2,
                            BackgroundColor = Color.FromArgb("#1485a5"),
                        }
                    }
                    };
                    var tapGesture = new TapGestureRecognizer();
                    tapGesture.Tapped += async (s, e) =>
                    {

                        var idPMTSelecionado = item.idPMT; 

                        await Application.Current.MainPage.Navigation.PushAsync(
                            new PMTsDetalhes(idColaborador, Token, mesAnoFormatado, nome_abreviado, idPMTSelecionado)); 
                    };
                    imageAndTextLayout.GestureRecognizers.Add(tapGesture);

                    StackPMTs.Children.Add(entryItemLayout);
                }
            }
            else
            {
                StackPMTs.Children.Add(new Label { Text = "Nenhuma notificação encontrada." });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Falha ao carregar notificações: " + ex.Message, "OK");
        }
    }
}