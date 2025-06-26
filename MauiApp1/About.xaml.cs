using System.Globalization;
using GH_Metodos;
using Microsoft.Maui.Controls;

namespace MauiApp1;

public partial class About : ContentPage
{
    private Service6SoapClient _service = new Service6SoapClient(new Service6SoapClient.EndpointConfiguration());
    private string versao;

    public About()
    {
        InitializeComponent();
        _ = LoadServicosAsync();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage());
    }

    private async Task LoadServicosAsync()
    {
        try
        {
            var resposta = await _service.GetVersaoAndroidAsync();
            var versaoAndroid = resposta?.Body?.GetVersaoAndroidResult?.versao;

            if (!string.IsNullOrEmpty(versaoAndroid))
            {
                versaoandroid.Text = versaoAndroid;
            }
            else
            {
                await DisplayAlert("Erro", "Não foi possível obter a versão do Android.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Dispatcher.DispatchAsync(async () =>
            {
                await DisplayAlert("Erro", $"Ocorreu um erro ao carregar os serviços: {ex.Message}", "OK");
            });
        }
    }

    private async void EmailLabel_Tapped(object sender, EventArgs e)
    {
        try
        {
            await Launcher.OpenAsync("mailto:geral@risi.pt");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível abrir o cliente de e-mail: {ex.Message}", "OK");
        }
    }

    private async void WebsiteLabel_Tapped(object sender, EventArgs e)
    {
        try
        {
            await Launcher.OpenAsync("http://www.risi.pt");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Não foi possível abrir o navegador: {ex.Message}", "OK");
        }
    }
}