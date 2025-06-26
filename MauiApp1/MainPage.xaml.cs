using CommunityToolkit.Maui.Views;
using GH_Metodos;
using MauiApp1;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.ServiceModel;
using System.Text.Json;
using System.Xml.Linq;
using MauiApp1.Services;
using Microsoft.Maui.Graphics; // Adicionado para Colors

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        private Service6SoapClient _service;
        private Entidade _entidadeSelecionada = null;
        private readonly ICalendarService _calendarService;

        private const string UsernameKey = "Username";
        private const string PasswordKey = "Password";
        private const string EntidadeKey = "LastSelectedEntidade";
        private const string RememberMeKey = "RememberMe";
        private int tipoaut;
        private bool variostipoaut = false;
        private bool mostralabel = false;
        private const string SelecionarEntidades = "Selecionar Entidade";
        private const string Acerca = "Acerca";
        public const string OpcaoCancelar = "Cancelar";

        public MainPage()
        {
            InitializeComponent();
            _service = new Service6SoapClient(new Service6SoapClient.EndpointConfiguration());

            var serviceProvider = App.Current.Handler.MauiContext.Services;
            _calendarService = serviceProvider.GetService<ICalendarService>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadPreferences();
        }

        private void LoadPreferences()
        {
            bool rememberMe = Preferences.Get(RememberMeKey, false);
            RememberMeSwitch.IsToggled = rememberMe;

            // Adicionado: Força a atualização da cor do Switch.
            if (rememberMe)
            {
                RememberMeSwitch.OnColor = Colors.Green;
            }

            if (rememberMe)
            {
                UsernameEntry.Text = Preferences.Get(UsernameKey, string.Empty);
                PasswordEntry.Text = Preferences.Get(PasswordKey, string.Empty);
            }
            else
            {
                UsernameEntry.Text = string.Empty;
                PasswordEntry.Text = string.Empty;
            }

            string savedEntidadeJson = Preferences.Get(EntidadeKey, string.Empty);
            if (!string.IsNullOrEmpty(savedEntidadeJson))
            {
                try
                {
                    var savedEntidade = JsonSerializer.Deserialize<Entidade>(savedEntidadeJson);
                    if (savedEntidade != null)
                    {
                        UpdateUIForSelectedEntidade(savedEntidade);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erro ao deserializar entidade: {ex.Message}");
                }
            }
        }

        private void UpdateUIForSelectedEntidade(Entidade entidade)
        {
            _entidadeSelecionada = entidade;
            NomeBotao.Text = entidade.descEntidade;

            if (entidade.autenticacaoNormal && entidade.autenticacaoActiveDirectory)
            {
                variostipoaut = true;
                AuthTypeLabel.Text = "Login Com Active Directory";
                tipoaut = 1;
                AuthTypeLabel.IsVisible = true;
                UsernameEntry.Placeholder = "Nrº Mecanográfico";
                UsernameEntry.Keyboard = Keyboard.Numeric;
            }
            else if (!entidade.autenticacaoNormal && entidade.autenticacaoActiveDirectory)
            {
                variostipoaut = false;
                tipoaut = 1;
                AuthTypeLabel.IsVisible = false;
                UsernameEntry.Placeholder = "Nrº Mecanográfico";
                UsernameEntry.Keyboard = Keyboard.Numeric;
            }
            else if (entidade.autenticacaoNormal && !entidade.autenticacaoActiveDirectory)
            {
                variostipoaut = false;
                tipoaut = 2;
                AuthTypeLabel.IsVisible = false;
                UsernameEntry.Placeholder = "Login AD";
                UsernameEntry.Keyboard = Keyboard.Default;
            }

            Preferences.Set("tipoaut", tipoaut.ToString());
            Remember.tipoaut = tipoaut;
            Preferences.Set("site", entidade.urlSite);
            Remember.site = entidade?.urlSite?.ToString() ?? string.Empty;
        }

        private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            string acao = await DisplayActionSheet(
              "Menu de Opções",
              OpcaoCancelar,
              null,
              SelecionarEntidades,
              Acerca);

            if (!string.IsNullOrEmpty(acao))
            {
                switch (acao)
                {
                    case SelecionarEntidades:
                        await MostrarPopupEntidades();
                        break;
                    case Acerca:
                        await Navigation.PushAsync(new About());
                        break;
                    case OpcaoCancelar:
                        break;
                }
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;
            string deviceID = "12345";
            string deviceOS = DeviceInfo.Platform.ToString();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Atenção", "Por favor, preencha o login e a password.", "OK");
                return;
            }

            if (_entidadeSelecionada == null)
            {
                await DisplayAlert("Erro", "Por favor, selecione uma entidade antes de entrar.", "OK");
                return;
            }

            try
            {
                var send = await _service.LoginWithADAsync(tipoaut, username, password, false, deviceID, deviceOS);
                var resp = send?.Body?.LoginWithADResult;

                if (resp != null && !string.IsNullOrEmpty(resp.token))
                {
                    if (RememberMeSwitch.IsToggled)
                    {
                        Preferences.Set(UsernameKey, username);
                        Preferences.Set(PasswordKey, password);
                        Preferences.Set(RememberMeKey, true);
                    }
                    else
                    {
                        Preferences.Remove(UsernameKey);
                        Preferences.Remove(PasswordKey);
                        Preferences.Set(RememberMeKey, false);
                    }

                    Remember.Username = username;
                    Remember.Password = password;

                    if (_calendarService == null)
                    {
                        await DisplayAlert("Erro", "Serviço de calendário não disponível. Por favor, contacte o suporte.", "OK");
                        return;
                    }
                    await Navigation.PushAsync(new OptionsPage(resp.idColaborador, resp.token, resp.nomeAbreviado, _calendarService));
                }
                else
                {
                    await DisplayAlert("Erro de Login", "Login ou password inválidos. Verifique as suas credenciais.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao tentar fazer login: {ex.Message}", "OK");
            }
        }

        private void RememberMeSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            bool isToggled = e.Value;
            Preferences.Set(RememberMeKey, isToggled);

            if (!isToggled)
            {
                Preferences.Remove(UsernameKey);
                Preferences.Remove(PasswordKey);
                UsernameEntry.Text = string.Empty;
                PasswordEntry.Text = string.Empty;
            }
        }

        private async void OnMostrarEntidadesClicked(object sender, EventArgs e)
        {
            await MostrarPopupEntidades();
        }

        private async Task MostrarPopupEntidades()
        {
            try
            {
                var binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

                var endpoint = new EndpointAddress("https://sites.risi.pt/wsGHMobile4Testes/Service6.asmx");
                var _client = new Service6SoapClient(binding, endpoint);

                var result = await _client.GetEntidadesAsync(null);
                var entidades = result.Body.GetEntidadesResult.aEntidades.ToList();

                if (entidades != null && entidades.Count > 0)
                {
                    var popup = new SelecioneEntidadePopup(entidades);
                    var entidadeSelecionada = await this.ShowPopupAsync(popup) as Entidade;

                    if (entidadeSelecionada != null)
                    {
                        UpdateUIForSelectedEntidade(entidadeSelecionada);

                        string entidadeJson = JsonSerializer.Serialize(entidadeSelecionada);
                        Preferences.Set(EntidadeKey, entidadeJson);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao obter entidades: {ex.Message}", "OK");
            }
        }

        private void AuthTypeLabel_Clicked(object sender, EventArgs e)
        {
            if (!variostipoaut)
            {
                return;
            }

            mostralabel = !mostralabel;

            if (mostralabel)
            {
                AuthTypeLabel.Text = "Login Com Active Directory";
                UsernameEntry.Placeholder = "Nrº Mecanográfico";
                UsernameEntry.Keyboard = Keyboard.Numeric;
                tipoaut = 1;
            }
            else
            {
                AuthTypeLabel.Text = "Login Normal";
                UsernameEntry.Placeholder = "Login AD";
                UsernameEntry.Keyboard = Keyboard.Default;
                tipoaut = 2;
            }
            UsernameEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;
            Preferences.Set("tipoaut", tipoaut.ToString());
            Remember.tipoaut = tipoaut;
        }
    }
}