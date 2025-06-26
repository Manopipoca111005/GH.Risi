using GH_Metodos;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using System;
using System.Threading.Tasks;

namespace MauiApp1;

public partial class ArquivarNotificacoes : ContentPage
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    private readonly int idColaborador;
    private readonly string Token;
    private readonly bool novas = false;
    private List<(int idNotificacao, Switch switchControl)> notificacoesComSwitch = new();

    public ArquivarNotificacoes(int id_colaborador, string token)
    {
        InitializeComponent();
        idColaborador = id_colaborador;
        Token = token;
        _ = CarregarNotificacoesAsync();
    }

    private async Task CarregarNotificacoesAsync()
    {
        try
        {
            var resposta = await _service.GetNotificacoesAsync(idColaborador, Token, novas);
            var notificacoes = resposta?.Body?.GetNotificacoesResult?.aNotificacoes;

            StackNotificacoes.Children.Clear();

            if (notificacoes != null && notificacoes.Length > 0)
            {
                for (int i = 0; i < notificacoes.Length; i++)
                {
                    var item = notificacoes[i];
                    // Skip if an item in the array is null (optional, but good for safety)
                    if (item == null) continue;

                    var switchArquivar = new Switch()
                    {
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.End,
                    };
                    notificacoesComSwitch.Add((item.idNotificacao, switchArquivar));

                    // Ensure item.mensagem is not null before calling ObterResumoMensagem
                    string mensagemPreview = (item.mensagem != null)
                                           ? Notificacoes.ObterResumoMensagem(item.mensagem, 3)
                                           : "Mensagem indisponível";

                    var notificacaoDetailsLayout = new VerticalStackLayout
                    {
                        Spacing = 2,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label { Text = item.remetente ?? "N/D", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#007BA7")},
                            new Label { Text = item.data ?? "N/D", TextColor = Color.FromArgb("#007BA7")},
                            new Label { Text = item.assunto ?? "N/D", TextColor = Color.FromArgb("#007BA7")},
                            new Label { Text = mensagemPreview, FontSize = 13}
                        }
                    };

                    var notificationItemLayout = new Grid
                    {
                        Padding = new Thickness(10, 5),
                        ColumnSpacing = 10,
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = GridLength.Auto }
                        },
                    };

                    Grid.SetColumn(notificacaoDetailsLayout, 0);
                    notificationItemLayout.Children.Add(notificacaoDetailsLayout);

                    Grid.SetColumn(switchArquivar, 1);
                    notificationItemLayout.Children.Add(switchArquivar);

                    // Add the notification item layout ONCE
                    StackNotificacoes.Children.Add(notificationItemLayout);

                    // Add separator, but not after the last item
                    if (i < notificacoes.Length - 1)
                    {
                        var separator = new BoxView
                        {
                            HeightRequest = 2,
                            BackgroundColor = Color.FromArgb("#1485a5"),
                            Margin = new Thickness(0, 5, 0, 5)
                        };
                        StackNotificacoes.Children.Add(separator);
                    }
                }
            }
            else
            {
                StackNotificacoes.Children.Add(new Label { Text = "Nenhuma notificação encontrada." });
            }
        }
        catch (Exception ex)
        {
            // For more detailed debugging, you can print ex.ToString() to the debug console
            // System.Diagnostics.Debug.WriteLine(ex.ToString());
            await DisplayAlert("Erro", "Falha ao carregar notificações: " + ex.Message, "OK");
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        var idsParaArquivar = notificacoesComSwitch
        .Where(t => t.switchControl.IsToggled)
        .Select(t => t.idNotificacao)
        .ToList();

        if (idsParaArquivar.Count == 0)
        {
            await DisplayAlert("Aviso", "Nenhuma notificação selecionada.", "OK");
            return;
        }

        string strIDs = string.Join(",", idsParaArquivar);

        try
        {
            var response = await _service.DelNotificacaoAsync(idColaborador, Token, strIDs);
            var result = response.Body.DelNotificacaoResult;

            if (result.erro == 0)
            {
                await DisplayAlert("Sucesso", "Notificações arquivadas com sucesso.", "OK");
                notificacoesComSwitch.Clear();
                await CarregarNotificacoesAsync();
            }
            else
            {
                await DisplayAlert("Erro", "Falha ao arquivar: " + result.erroMensagem, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Erro na comunicação: " + ex.Message, "OK");
        }

    }
}