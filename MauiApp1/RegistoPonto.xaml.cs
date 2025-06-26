using GH_Metodos;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Platform;
using System;
using System.Globalization;
using System.IO;

namespace MauiApp1
{
    public partial class RegistoPonto : ContentPage
    {
        public const string OpcaoDefinicoes = "Defini��es";
        public const string OpcaoSair = "Sair da app";
        public const string OpcaoCancelar = "Cancelar";
        public readonly string Token;
        public readonly int IdColaborador;
        public string coordenadasGPS;

        private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);

        public RegistoPonto(int idcolaborador, string token)
        {
            InitializeComponent();
            IdColaborador = idcolaborador;
            Token = token;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadOpenFreeMapFromFile();

            mapa.Navigating += (s, e) =>
            {
                if (e.Url.StartsWith("invoke://"))
                {
                    e.Cancel = true;
                    var coords = Uri.UnescapeDataString(e.Url.Replace("invoke://", ""));
                    coordenadasGPS = coords;
                    Console.WriteLine("Nova localiza��o: " + coordenadasGPS);
                }
            };
        }

        private async Task LoadOpenFreeMapFromFile()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("registoponto.html");
            using var reader = new StreamReader(stream);
            string html = await reader.ReadToEndAsync();

            mapa.Source = new HtmlWebViewSource { Html = html };

            await Task.Delay(1000);
            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Erro", "Erro", "OK");
                    return;
                }

                var location = await Geolocation.GetLastKnownLocationAsync() ?? await Geolocation.GetLocationAsync();


                if (location != null)
                {
                    Console.WriteLine($" Latitude: {location.Latitude}, Longitude: {location.Longitude}");
                    coordenadasGPS = $"{location.Latitude.ToString(CultureInfo.InvariantCulture)},{location.Longitude.ToString(CultureInfo.InvariantCulture)}";

                    double lat = location.Latitude;
                    double lon = location.Longitude;

                    await Task.Delay(500);

                    string js = $"initMap({lat.ToString(CultureInfo.InvariantCulture)}, {lon.ToString(CultureInfo.InvariantCulture)});";
                    await mapa.EvaluateJavaScriptAsync(js);

                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Ocorreu um erro ao obter a localiza��o: {ex.Message}", "OK");
            }
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await DisplayAlert(null, "Recarregando mapa...", "OK");
            await LoadOpenFreeMapFromFile();
        }

        private async void OnSendClicked(object sender, EventArgs e)
        {
            try
            {
                
                var location = await Geolocation.GetLastKnownLocationAsync() ?? await Geolocation.GetLocationAsync();

                if (location == null)
                {
                    await DisplayAlert("Erro", "N�o foi poss�vel obter a sua localiza��o atual para enviar o ponto.", "OK");
                    return;
                }

                int idColaborador = IdColaborador;
                string token = Token;
                

                var resposta = await _service.SetRegistoPontoAsync(idColaborador, token, coordenadasGPS);

                var resultado = resposta?.Body?.SetRegistoPontoResult;

                if (resultado != null)
                {
                    if (resultado.erro == 0)
                    {
                        await DisplayAlert("Sucesso", "Ponto registado com sucesso!", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Erro do Servi�o", resultado.erroMensagem, "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Erro", "Resposta inv�lida do servi�o ao registar o ponto.", "OK");
                }
            }
            catch (FeatureNotSupportedException)
            {
                await DisplayAlert("Erro", "Geolocaliza��o n�o suportada neste dispositivo.", "OK");
            }
            catch (PermissionException)
            {
                await DisplayAlert("Erro", "Permiss�o de localiza��o n�o concedida. Por favor, habilite a permiss�o nas configura��es do seu dispositivo.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Ocorreu um erro ao enviar a localiza��o: {ex.Message}", "OK");
            }
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
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
                        break;
                }
            }
        }
    }
}