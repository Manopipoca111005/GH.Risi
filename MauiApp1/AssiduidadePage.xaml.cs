using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MauiApp1;
using GH_Metodos; // Certifique-se que o seu Service6SoapClient está neste namespace ou adicione o using correto
using System.Diagnostics;

namespace MauiApp1
{
    public partial class AssiduidadePage : ContentPage, INotifyPropertyChanged
    {
        private readonly int _idColaborador;
        private readonly string _token;
        private readonly string _nomeAbreviado;
        public const string OpcaoDefinicoes = "Definições";
        public const string OpcaoSair = "Sair da app";
        public const string OpcaoCancelar = "Cancelar";

        private ObservableCollection<AssiduidadeModel> _listaAssiduidade;
        public ObservableCollection<AssiduidadeModel> ListaAssiduidade
        {
            get => _listaAssiduidade;
            set { _listaAssiduidade = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotBusy)); }
        }
        public bool IsNotBusy => !IsBusy;

        public AssiduidadePage(int idColaborador, string token,string nomeabreviado)
        {
            InitializeComponent();
            _idColaborador = idColaborador;
            _token = token;
            _nomeAbreviado= nomeabreviado;
            ListaAssiduidade = new ObservableCollection<AssiduidadeModel>();
            this.BindingContext = this;
            InitializeDatePickersAndLoadData();
        }

        public AssiduidadePage()
        {
            InitializeComponent();
            _idColaborador = _idColaborador;
            _token = _token;
            ListaAssiduidade = new ObservableCollection<AssiduidadeModel>();
            this.BindingContext = this;
            InitializeDatePickersAndLoadData();
        }

        private void InitializeDatePickersAndLoadData()
        {
            DateDePicker.Date = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateAtePicker.Date = DateTime.Today;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CarregarDadosAsync();
        }

        private async void OnSearchTapped(object sender, TappedEventArgs e)
        {
            await CarregarDadosAsync();
        }

        public async Task CarregarDadosAsync()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            var itensProcessados = new List<AssiduidadeModel>();

            try
            {
                var client = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
                string dataInicioApi = DateDePicker.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                string dataFimApi = DateAtePicker.Date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                var resultado = await client.GetRegistosPontoAsync(
                    _idColaborador,
                    _token,
                    dataInicioApi,
                    dataFimApi
                );

                if (resultado?.Body?.GetRegistosPontoResult?.aRegistoPonto != null)
                {
                    var listaApi = resultado.Body.GetRegistosPontoResult.aRegistoPonto;
                    foreach (var r in listaApi)
                    {
                        itensProcessados.Add(new AssiduidadeModel
                        {
                            Data = r.data,
                            E1 = r.e1,
                            S1 = r.s1,
                            E2 = r.e2,
                            S2 = r.s2,
                            E3 = r.e3,
                            S3 = r.s3,
                            E4 = r.e4,
                            S4 = r.s4,
                            TemOcorrencia = r.em1 || r.sm1 || r.em2 || r.sm2 || r.em3 || r.sm3 || r.em4 || r.sm4 || r.obsSupervisor || r.obsColaborador || r.obsResponsavel
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AssiduidadePage.CarregarDadosAsync] Exception: {ex.Message}");
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Erro de Serviço", $"Falha ao comunicar com o serviço: {ex.Message}", "OK");
                }
            }
            finally
            {
                ListaAssiduidade.Clear();
                foreach (var item in itensProcessados)
                {
                    ListaAssiduidade.Add(item);
                }
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is AssiduidadeModel itemSelecionado)
            {
                if (ListaAssiduidade != null && ListaAssiduidade.Count > 0)
                {
                    await Navigation.PushAsync(new AdicionarOcorrencia(ListaAssiduidade, _idColaborador, _token, itemSelecionado.Data, _nomeAbreviado));
                }
            }
        }
    }
}