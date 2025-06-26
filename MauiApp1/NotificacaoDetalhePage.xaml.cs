namespace MauiApp1;

public partial class NotificacaoDetalhePage : ContentPage
{
    public const string OpcaoDefinicoes = "Defini��es";
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";
    public NotificacaoDetalhePage(string remetente, string data, string assunto, string mensagem)
    {
        InitializeComponent();

        LabelRemetente.Text = remetente;
        LabelData.Text = data;
        LabelAssunto.Text = assunto;
        LabelMensagem.Text = mensagem;
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        string acao = await DisplayActionSheet(
            "Menu de Op��es",
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
                    await DisplayAlert("A��o", "Opera��o cancelada.", "OK");
                    break;
            }
        }
    }
}