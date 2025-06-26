using System.Collections.ObjectModel;
using GH_Metodos;
using Microsoft.Maui.Controls;
using System;
using System.Globalization;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Text;

namespace MauiApp1;

public partial class Disponibilidades : ContentPage, INotifyPropertyChanged
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    private readonly int IdColaborador;
    private readonly string Token;
    public const string OpcaoDefinicoes = "Definições";
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";

    private ObservableCollection<DisponibilidadeItem> _disponibilidadesList;
    private bool _isFechadoTabActive = false;

    private string _servicoMesAnoLabelText = "Carregando serviço...";
    public string ServicoMesAnoLabelText
    {
        get => _servicoMesAnoLabelText;
        set
        {
            if (_servicoMesAnoLabelText != value)
            {
                _servicoMesAnoLabelText = value;
                OnPropertyChanged();
            }
        }
    }

    private string _incrementoLabelText = "Carregando incremento...";
    public string IncrementoLabelText
    {
        get => _incrementoLabelText;
        set
        {
            if (_incrementoLabelText != value)
            {
                _incrementoLabelText = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class DisponibilidadeItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int idDisc { get; set; }

        private int _estado;
        public int Estado
        {
            get => _estado;
            set
            {
                if (_estado != value)
                {
                    _estado = value;
                    OnPropertyChanged(nameof(_estado));
                    OnPropertyChanged(nameof(EstadoImagem));
                }
            }
        }

        public int EstadoOriginal { get; set; }

        public string EstadoImagem =>
            Estado switch
            {
                0 => "nao_aceite.png",
                -1 => "interrogacao.png",
                1 => "aceite.png",
                _ => "interrogacao.png"
            };

        public int Dia { get; set; }

     

        public string Prev { get; set; }
        public string ItemDisponibilidade { get; set; }

        public int mes { get; set; }

        public int ano { get; set; }

        public void AlternarDisponivel()
        {
            Estado = Estado switch
            {
                1 => 0,
                0 => -1,
                -1 => 1,
                _ => -1
            };
        }

        void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public ICommand ChangeEstadoCommand { get; private set; }

    public Disponibilidades(int idcolaborador, string token)
    {
        InitializeComponent();
        IdColaborador = idcolaborador;
        Token = token;

        _disponibilidadesList = new ObservableCollection<DisponibilidadeItem>();
        disponibilidadesCollectionView.ItemsSource = _disponibilidadesList;

        this.BindingContext = this;

        UpdateTabColors();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDisponibilidades(fechado: _isFechadoTabActive);
    }

    private async Task LoadDisponibilidades(bool fechado)
    {
        try
        {
            var resposta = await _service.GetDisponibilidades2Async(IdColaborador, Token, fechado);
            var serviceResult = resposta?.Body?.GetDisponibilidades2Result;

            _disponibilidadesList.Clear();

            if (serviceResult != null)
            {
                if (serviceResult._erro != null && serviceResult._erro.erro != 0)
                {
                    await DisplayAlert("Erro do Serviço", serviceResult._erro.erroMensagem, "OK");
                    ServicoMesAnoLabelText = "Erro ao carregar serviço";
                    IncrementoLabelText = "Erro ao carregar incremento";
                    return;
                }

                if (serviceResult.aDisponibilidades != null && serviceResult.aDisponibilidades.Length > 0)
                {
                    var firstSoapItem = serviceResult.aDisponibilidades[0];

                    string monthName = new DateTime(firstSoapItem.ano, firstSoapItem.mes, 1).ToString("MMM", new CultureInfo("pt-PT")).ToUpper().Replace(".", "");
                    ServicoMesAnoLabelText = $"SERVIÇO {(firstSoapItem.descServico?.Replace("SERVIÇO ", "", StringComparison.OrdinalIgnoreCase).Split(' ')[0] ?? "N/A").ToUpper()} - {monthName}/{firstSoapItem.ano}";

                    IncrementoLabelText = (firstSoapItem.descDisponibilidadeMotivo ?? "INCREMENTO INDISPONÍVEL").ToUpper();

                    

                    foreach (var soapItem in serviceResult.aDisponibilidades)
                    {
                        foreach (var item in soapItem.aDisponibilidadesTurnos)
                        {
                            _disponibilidadesList.Add(new DisponibilidadeItem
                            {
                                Dia = item.dia,
                                Prev = item.turnosPrevistos,
                                ItemDisponibilidade = item.descTurno,
                                Estado = item.disponivel,
                                EstadoOriginal = item.disponivel,
                                mes = soapItem.mes,
                                ano = soapItem.ano,
                                idDisc = item.idDisponibilidadeTurno,
                               
                            });
                         
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Informação", "Nenhuma disponibilidade encontrada para exibir.", "OK");
                    ServicoMesAnoLabelText = "Nenhum Serviço Disponível";
                    IncrementoLabelText = "Nenhum Incremento Disponível";
                }
            }
            else
            {
                await DisplayAlert("Erro de Dados", "Nenhum resultado válido retornado do serviço.", "OK");
                ServicoMesAnoLabelText = "Erro de dados do serviço";
                IncrementoLabelText = "Erro de dados do incremento";
            }
        }
        catch (System.ServiceModel.FaultException faultEx)
        {
            await DisplayAlert("Erro SOAP", $"Ocorreu um erro no serviço: {faultEx.Message}", "OK");
            ServicoMesAnoLabelText = "Erro de rede/serviço";
            IncrementoLabelText = "Erro de rede/serviço";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro ao carregar disponibilidades: {ex.Message}", "OK");
            ServicoMesAnoLabelText = "Erro geral";
            IncrementoLabelText = "Erro geral";
        }
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

    private void OnTabButtonClicked(object sender, EventArgs e)
    {
        Button clickedButton = sender as Button;

        if (clickedButton == btnEmAberto)
        {
            _isFechadoTabActive = false;
        }
        else if (clickedButton == btnFechadas)
        {
            _isFechadoTabActive = true;
        }

        UpdateTabColors();
        _ = LoadDisponibilidades(fechado: _isFechadoTabActive);
    }

    private void UpdateTabColors()
    {
        Color primaryColor;
        Color secondaryColor;

        if (Application.Current != null && Application.Current.Resources.TryGetValue("PrimaryColor", out object primaryObj) && primaryObj is Color)
        {
            primaryColor = (Color)primaryObj;
        }
        else
        {
            primaryColor = Color.FromArgb("#00AEEF");
        }

        if (Application.Current != null && Application.Current.Resources.TryGetValue("SecondaryColor", out object secondaryObj) && secondaryObj is Color)
        {
            secondaryColor = (Color)secondaryObj;
        }
        else
        {
            secondaryColor = Colors.DeepSkyBlue;
        }

        if (!_isFechadoTabActive)
        {
            btnEmAberto.BackgroundColor = primaryColor;
            btnFechadas.BackgroundColor = secondaryColor;
        }
        else
        {
            btnEmAberto.BackgroundColor = secondaryColor;
            btnFechadas.BackgroundColor = primaryColor;
        }
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        try
        {
            var lista = disponibilidadesCollectionView.ItemsSource as IEnumerable<DisponibilidadeItem>;

            if (lista == null || !lista.Any())
            {
                await DisplayAlert("Aviso", "Não há dados para guardar.", "OK");
                return;
            }

            var modificados = lista
                .Where(d => d.Estado != d.EstadoOriginal && d.Estado >= -1 && d.Estado <= 1)
                .ToList();

            if (!modificados.Any())
            {
                await DisplayAlert("Info", "Nenhuma alteração a guardar.", "OK");
                return;
            }

            foreach (var item in modificados)
            {
                var id = item.idDisc.ToString();
                var novoEstado = item.Estado.ToString();

                var resultado = await _service.SetDisponibilidadesTurnosAsync(IdColaborador, Token, id, novoEstado);
                var retorno = resultado?.Body?.SetDisponibilidadesTurnosResult;

                if (retorno?.erro != 0)
                {
                    await DisplayAlert("Erro", $"Erro ao guardar disponibilidade para o dia {item.Dia} ({item.ItemDisponibilidade}): {retorno.erroMensagem}", "OK");

                }
            }

            await DisplayAlert("Sucesso", "Disponibilidades atualizadas com sucesso!", "OK");
            await LoadDisponibilidades(fechado: _isFechadoTabActive); // Recarrega lista atualizada
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Erro ao guardar alterações: " + ex.Message, "OK");
        }
    }

    private void TapGestureRecognizer_Tapped_2(object sender, TappedEventArgs e)
    {
        if (sender is Image img && img.BindingContext is DisponibilidadeItem vm)
        {
            vm.AlternarDisponivel();
        }
    }
}