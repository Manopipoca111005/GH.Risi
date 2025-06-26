namespace MauiApp1;
using GH_Metodos;

public partial class Notificacoes : ContentPage
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);

    // Exemplo: inicialize esses campos com seus dados reais
    private readonly int idColaborador;
    private readonly string Token;
    private readonly bool novas = false;
    private readonly string nome_abreviado;
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";

    public Notificacoes(int id_colaborador, string token, string NomeAbreviado)
    {
        InitializeComponent();
        idColaborador = id_colaborador;
        Token = token;
        nome_abreviado = NomeAbreviado;
        _ = CarregarNotificacoesAsync();

    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new AdicionarNotificacao(idColaborador, Token, nome_abreviado));
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
                foreach (var item in notificacoes)
                {
                    string mensagemPreview = Notificacoes.ObterResumoMensagem(item.mensagem, 20);
                    var border = new VerticalStackLayout
                    {
                        Spacing = 4,
                        Padding = new Thickness(0, 2),
                        Children =
                {
                    new Label { Text = item.remetente, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#007BA7")},
                    new Label { Text = item.data , TextColor = Color.FromArgb("#007BA7")},
                    new Label { Text = item.assunto, TextColor = Color.FromArgb("#007BA7")},
                    new Label { Text = mensagemPreview, FontSize = 13},
                    new BoxView
                    {
                       HeightRequest= 2,
                       BackgroundColor= Color.FromArgb("#1485a5"),
                    }
                }
                    };

                    // Criar e associar o TapGestureRecognizer
                    var tapGesture = new TapGestureRecognizer();
                    tapGesture.Tapped += async  (s, e) =>
                    {
                        await Application.Current.MainPage.Navigation.PushAsync(
                        new NotificacaoDetalhePage(item.remetente, item.data, item.assunto, item.mensagem));
                    };
                    border.GestureRecognizers.Add(tapGesture);

                    StackNotificacoes.Children.Add(border);
                }
            }
            else
            {
                StackNotificacoes.Children.Add(new Label { Text = "Nenhuma notificação encontrada." });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Falha ao carregar notificações: " + ex.Message, "OK");
        }


    }

    public static string ObterResumoMensagem(string mensagem, int numeroPalavras)
    {
        if (string.IsNullOrWhiteSpace(mensagem))
            return string.Empty;


        var palavras = mensagem.Split(' ');      
        return string.Join(" ", palavras.Take(numeroPalavras)) + (palavras.Length > numeroPalavras? "...":"");
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new ArquivarNotificacoes(idColaborador, Token));
    }
}
