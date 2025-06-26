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
        public const string OpcaoDefinicoes = "Definições";
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
                    Console.WriteLine("Nova localização: " + coordenadasGPS);
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
                await DisplayAlert("Erro", $"Ocorreu um erro ao obter a localização: {ex.Message}", "OK");
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
                    await DisplayAlert("Erro", "Não foi possível obter a sua localização atual para enviar o ponto.", "OK");
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
                        await DisplayAlert("Erro do Serviço", resultado.erroMensagem, "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Erro", "Resposta inválida do serviço ao registar o ponto.", "OK");
                }
            }
            catch (FeatureNotSupportedException)
            {
                await DisplayAlert("Erro", "Geolocalização não suportada neste dispositivo.", "OK");
            }
            catch (PermissionException)
            {
                await DisplayAlert("Erro", "Permissão de localização não concedida. Por favor, habilite a permissão nas configurações do seu dispositivo.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Ocorreu um erro ao enviar a localização: {ex.Message}", "OK");
            }
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
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
    }
}