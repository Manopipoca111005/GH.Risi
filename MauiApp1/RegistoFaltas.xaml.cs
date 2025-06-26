using GH_Metodos;
using Microsoft.Maui.Controls;
namespace MauiApp1;

public partial class RegistoFaltas : ContentPage
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    private readonly int id_colaborador;
    private readonly string token;
    private readonly string nome_abreviado;
    public const string OpcaoDefinicoes = "Definições";
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";
    private int IdPMT;
    private bool pendentes=true;
    private bool pendente = false;

    public RegistoFaltas(int IdColaborador, string Token, string NomeAbreviado)
	{
		InitializeComponent();
        id_colaborador = IdColaborador;
        token = Token;
        nome_abreviado = NomeAbreviado;
        CarregarFaltasAsync();
        
        
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

    private async void NovoPedidoButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PedidosFaltas(id_colaborador, token, nome_abreviado));
    }

    private async Task CarregarFaltasAsync()
    {
        try
        {
            var resposta = await _service.GetListaFaltasAsync(id_colaborador, token, pendentes);
            var faltas = resposta?.Body?.GetListaFaltasResult?.aFaltas;

            StackFaltas.Children.Clear();

            if (faltas != null && faltas.Length > 0)
            {
                foreach (var item in faltas)
                {
                    var line1Grid = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
                    };

                    var numeroLabel = new Label
                    {
                        Text = item.numero,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#007BA7"),
                        HorizontalOptions = LayoutOptions.Start
                    };
                    Grid.SetColumn(numeroLabel, 0);

                    var dataInicioLabel = new Label
                    {
                        Text = item.dataInicio,
                        TextColor = Color.FromArgb("#007BA7"),
                        HorizontalOptions = LayoutOptions.End
                    };
                    Grid.SetColumn(dataInicioLabel, 2);

                    line1Grid.Children.Add(numeroLabel);
                    line1Grid.Children.Add(dataInicioLabel);

                    var line2Grid = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
                    };

                    var descEstadoPedidoDocLabel = new Label
                    {
                        Text = item.descEstadoPedidoDoc,
                        TextColor = Color.FromArgb("#007BA7"),
                        HorizontalOptions = LayoutOptions.Start
                    };
                    Grid.SetColumn(descEstadoPedidoDocLabel, 0);

                    var dataFimLabel = new Label
                    {
                        Text = item.dataFim,
                        TextColor = Color.FromArgb("#007BA7"),
                        HorizontalOptions = LayoutOptions.End
                    };
                    Grid.SetColumn(dataFimLabel, 2);

                    line2Grid.Children.Add(descEstadoPedidoDocLabel);
                    line2Grid.Children.Add(dataFimLabel);

                    var descricaoLabel = new Label
                    {
                        Text = item.descricao,
                        TextColor = Color.FromArgb("#007BA7")
                    };

                    var border = new VerticalStackLayout
                    {
                        Spacing = 4,
                        Padding = new Thickness(16, 6),
                        Children =
            {
                line1Grid,
                line2Grid,
                descricaoLabel,
                new BoxView
                {
                    HeightRequest = 1,
                    BackgroundColor = Color.FromArgb("#1485a5"),
                    Margin = new Thickness(0, 4, 0, 0)
                }
            }
                    };
                    StackFaltas.Children.Add(border);
                }
            }
            else
            {
                StackFaltas.Children.Add(new Label
                {
                    Text = "Nenhuma falta encontrada.",
                    HorizontalOptions = LayoutOptions.Center,
                    Padding = new Thickness(0, 20)
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Falha ao carregar faltas: " + ex.Message, "OK");
        }


    }

    private async Task CarregarFaltaAsync()
    {
        try
        {
            var resposta = await _service.GetListaFaltasAsync(id_colaborador, token, pendente);
            var faltas = resposta?.Body?.GetListaFaltasResult?.aFaltas;

            StackFaltas.Children.Clear();

            if (faltas != null && faltas.Length > 0)
            {
                foreach (var item in faltas)
                {
                    var border = new VerticalStackLayout
                    {
                        Spacing = 4,
                        Padding = new Thickness(0, 2),
                        Children =
                {
                    new Label { Text = item.numero, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#007BA7")},
                    new Label { Text = item.descEstadoPedidoDoc , TextColor = Color.FromArgb("#007BA7")},
                    new Label { Text = item.descricao, TextColor = Color.FromArgb("#007BA7")},
                    new Label { Text = item.dataInicio,TextColor = Color.FromArgb("#007BA7")},
                    new Label { Text = item.dataFim, TextColor = Color.FromArgb("#007BA7")},
                    new BoxView
                    {
                       HeightRequest= 2,
                       BackgroundColor= Color.FromArgb("#1485a5"),
                    }
                }
                    };
                    StackFaltas.Children.Add(border);
                }

            }
            else
            {
                StackFaltas.Children.Add(new Label { Text = "Nenhuma falta encontrada." });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Falha ao carregar faltas: " + ex.Message, "OK");
        }


    }

    private void PendentesButton_Clicked(object sender, EventArgs e)
    {
        // Atualizar estilos dos botões
        CarregarFaltasAsync();
        PendentesButton.Style = (Style)Resources["SelectedTabButton"];
        HistoricoButton.Style = (Style)Resources["UnselectedTabButton"];

        PendentesButton.BackgroundColor = Colors.Blue;
        HistoricoButton.BackgroundColor = Color.FromArgb("#00AEEF");

        // Mostrar conteúdo correto
        PendentesContent.IsVisible = true;
        HistoricoContent.IsVisible = false;
    }

    private void HistoricoButton_Clicked(object sender, EventArgs e)
    {
        CarregarFaltaAsync();
        // Atualizar estilos dos botões
        HistoricoButton.Style = (Style)Resources["SelectedTabButton"];
        PendentesButton.Style = (Style)Resources["UnselectedTabButton"];

        HistoricoButton.BackgroundColor = Colors.Blue;
        PendentesButton.BackgroundColor = Color.FromArgb("#00AEEF");

        // Mostrar conteúdo correto
        HistoricoContent.IsVisible = true;
        PendentesContent.IsVisible = false;
    }
}