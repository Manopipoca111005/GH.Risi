namespace MauiApp1;

public partial class SettingsPage : ContentPage
{
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";
    public SettingsPage()
    {
        InitializeComponent();
        for (int i = 1; i <= 12; i++)
        {
            pickerMeses.Items.Add(i.ToString());
        }

        pickerMeses.SelectedIndex = 0;

        // ADIÇÃO: Carregar o estado do RememberMe ao inicializar a página
        LoadRememberMeState();
    }

    // NOVO MÉTODO: Para carregar o estado do RememberMe
    private void LoadRememberMeState()
    {
        bool rememberMe = Preferences.Get("RememberMe", false);
        RememberMeSwitch.IsToggled = rememberMe;
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

    private void RememberMeSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        if (RememberMeSwitch.IsToggled)
        {
            Preferences.Set("RememberMe", true);
            Preferences.Set("Username", Remember.Username);
            Preferences.Set("Password", Remember.Password);
        }
        else
        {
            Preferences.Remove("Username");
            Preferences.Remove("Password"); // Garantir que a password também é removida
            Preferences.Set("RememberMe", false);
        }
    }

    private void pickerMeses_SelectedIndexChanged(object sender, EventArgs e)
    {
        string valorSelecionado = pickerMeses.SelectedItem.ToString();
        Console.WriteLine($"Mês selecionado: {valorSelecionado}");
    }

    private void Automatic_Synchronization_Toggled(object sender, ToggledEventArgs e)
    {

    }

    private void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {

    }
}