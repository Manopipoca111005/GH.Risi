using System.Collections.ObjectModel;
using GH_Metodos;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Diagnostics;

namespace MauiApp1;

public class TrocaDiaItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string DayOfWeekString { get; set; }
    public DateTime DayNumberFormatted { get; set; }
    public string DiaTexto => DayNumberFormatted.Day.ToString("00");
    public string colab1background { get; set; }
    public string colab2background { get; set; }
    public string colabatualsigla { get; set; }
    public string id { get; set; }

    private string _colabselecionadoSigla;
    public string ColabselecionadoSigla
    {
        get => _colabselecionadoSigla;
        set
        {
            if (_colabselecionadoSigla != value)
            {
                _colabselecionadoSigla = value;
                OnPropertyChanged();
            }
        }
    }

    private bool _isColabAtualSelected;
    public bool IsColabAtualSelected
    {
        get => _isColabAtualSelected;
        set
        {
            if (_isColabAtualSelected != value)
            {
                _isColabAtualSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColabAtualImageSource));
            }
        }
    }

    private bool _isColab2Selected;
    public bool IsColab2Selected
    {
        get => _isColab2Selected;
        set
        {
            if (_isColab2Selected != value)
            {
                _isColab2Selected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Colab2ImageSource));
            }
        }
    }

    private bool _isSelectable;
    public bool IsSelectable
    {
        get => _isSelectable;
        set
        {
            if (_isSelectable != value)
            {
                _isSelectable = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColabAtualImageSource));
                OnPropertyChanged(nameof(Colab2ImageSource));
            }
        }
    }

    public string ColabAtualImageSource => IsColabAtualSelected ? "checked.png" : "unchecked.png";
    public string Colab2ImageSource => IsColab2Selected ? "checked_ver.png" : "unchecked_ver.png";
}

public partial class AdicionarTrocaPasso3 : ContentPage, INotifyPropertyChanged
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    public const string OpcaoDefinicoes = "Definiçőes";
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";
    public readonly int IdColaborador;
    public readonly string Token;
    public readonly int IdPMT;
    public int IdColabTroca;
    public string nomeabreviado;
    public string nomecolabtroca;
    public string Servico;

    private TrocaDiaItem _selectedColabAtualItem;
    private TrocaDiaItem _selectedColab2Item;

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private ObservableCollection<TrocaDiaItem> _trocaDias;
    public ObservableCollection<TrocaDiaItem> TrocaDias
    {
        get => _trocaDias;
        set
        {
            if (_trocaDias != value)
            {
                _trocaDias = value;
                OnPropertyChanged();
            }
        }
    }

    public AdicionarTrocaPasso3(int idcolaborador, string token, int idpmt, int idcolab, string nome_abreviado, string nomecolab2, string servico)
    {
        InitializeComponent();
        IdColaborador = idcolaborador;
        Token = token;
        IdPMT = idpmt;
        nomeabreviado = nome_abreviado;
        IdColabTroca = idcolab;
        nomecolabtroca = nomecolab2;
        lblColab.Text = nomeabreviado;
        lblcolab2.Text = nomecolabtroca;
        Servico = servico;

        TrocaDias = new ObservableCollection<TrocaDiaItem>();
        this.BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTrocaDias();
    }

    private async Task LoadTrocaDias()
    {
        try
        {
            TrocaDias.Clear();

            var responseTrocaDias = await _service.GetTrocasDiasAsync(IdColaborador, Token, IdPMT, IdColabTroca);

            var colab1DataByDate = new Dictionary<DateTime, (string sigla, string cor1)>();
            var colab2DataByDate = new Dictionary<DateTime, (string sigla, string cor2)>();

            if (responseTrocaDias?.Body?.GetTrocasDiasResult?.aDiasColabs != null)
            {
                foreach (var dia in responseTrocaDias.Body.GetTrocasDiasResult.aDiasColabs)
                {
                    if (DateTime.TryParse(dia.data, out DateTime date))
                    {
                        string siglaColab1 = dia.sigla1 ?? string.Empty;
                        colab1DataByDate[date.Date] = (siglaColab1, dia.cor1 ?? string.Empty);

                        string siglaColab2 = dia.sigla2 ?? string.Empty;
                        colab2DataByDate[date.Date] = (siglaColab2, dia.cor2 ?? string.Empty);
                    }
                }
            }

            var allDates = colab1DataByDate.Keys.Union(colab2DataByDate.Keys).OrderBy(d => d).ToList();

            foreach (var date in allDates)
            {
                string colab1Sigla = colab1DataByDate.TryGetValue(date, out var data1) ? data1.sigla : string.Empty;
                string colab1Cor = colab1DataByDate.TryGetValue(date, out data1) ? data1.cor1 : string.Empty;

                string colab2Sigla = colab2DataByDate.TryGetValue(date, out var data2) ? data2.sigla : string.Empty;
                string colab2Cor = colab2DataByDate.TryGetValue(date, out data2) ? data2.cor2 : string.Empty;

                var newItem = new TrocaDiaItem
                {
                    DayOfWeekString = date.ToString("ddd", new CultureInfo("pt-PT")).ToUpper().Substring(0, 1),
                    DayNumberFormatted = date,
                    colabatualsigla = colab1Sigla,
                    ColabselecionadoSigla = colab2Sigla,
                    IsSelectable = date.Date > DateTime.Today.Date && !string.IsNullOrEmpty(colab1Sigla) && !string.IsNullOrEmpty(colab2Sigla),
                    colab1background = colab1Cor,
                    colab2background = colab2Cor,
                };
                TrocaDias.Add(newItem);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro ao carregar os dados de troca: {ex.Message}", "OK");
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        string acao = await DisplayActionSheet(
            "Menu de Opçőes",
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

    private async void OnAnteriorClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AdicionarTrocaPasso2(IdColaborador, Token, IdPMT, nomeabreviado, Servico));
    }

    private async void OnSeguinteClicked(object sender, EventArgs e)
    {
        try
        {
            var selectedColabAtual = GetSelectedColabAtualItem();
            var selectedColab2 = GetSelectedColab2Item();

            // Validação básica de seleção
            if (selectedColabAtual == null || selectedColab2 == null)
            {
                await DisplayAlert("Atenção", "Por favor, selecione um dia para cada colaborador.", "OK");
                return;
            }

            Debug.WriteLine($"Tentando processar troca entre {nomeabreviado} e {nomecolabtroca}");
            Debug.WriteLine($"Dias selecionados: {selectedColabAtual.DayNumberFormatted} e {selectedColab2.DayNumberFormatted}");

            // Verificação adicional de dias válidos
            if (selectedColabAtual.DayNumberFormatted <= DateTime.Today ||
                selectedColab2.DayNumberFormatted <= DateTime.Today)
            {
                await DisplayAlert("Erro", "Não é possível selecionar dias passados ou o dia atual.", "OK");
                return;
            }

            // Converter datas para o formato esperado pelo serviço
            int dia1Formatted, dia2Formatted;
            try
            {
                dia1Formatted = int.Parse(selectedColabAtual.DayNumberFormatted.ToString("yyyyMMdd"));
                dia2Formatted = int.Parse(selectedColab2.DayNumberFormatted.ToString("yyyyMMdd"));
            }
            catch (FormatException)
            {
                await DisplayAlert("Erro", "Formato de data inválido.", "OK");
                return;
            }

            // Obter ID do dia para o colaborador atual - COM TRATAMENTO MELHORADO
            Debug.WriteLine($"Obtendo dias para {nomeabreviado} (ID: {IdColaborador})");
            var getDiasColabAtualResponse = await _service.GetDiasAsync(IdPMT, 0, IdColaborador, Token);

            if (getDiasColabAtualResponse?.Body?.GetDiasResult == null)
            {
                await DisplayAlert("Erro", "Resposta inválida do serviço para obter dias do colaborador atual.", "OK");
                return;
            }

            if (getDiasColabAtualResponse.Body.GetDiasResult._erro?.erro != 0)
            {
                string errorMsg = getDiasColabAtualResponse.Body.GetDiasResult._erro?.erroMensagem ??
                                "Erro desconhecido ao obter dias do colaborador atual";
                await DisplayAlert("Erro", errorMsg, "OK");
                return;
            }

            if (getDiasColabAtualResponse.Body.GetDiasResult.aDias == null ||
                !getDiasColabAtualResponse.Body.GetDiasResult.aDias.Any())
            {
                await DisplayAlert("Erro",
                    $"Nenhum dia marcado encontrado para {nomeabreviado} no período solicitado.",
                    "OK");
                return;
            }

            // Encontrar o dia específico selecionado
            var diaColabAtual = getDiasColabAtualResponse.Body.GetDiasResult.aDias
                .FirstOrDefault(d => DateTime.TryParse(d.data, out DateTime parsedDate) &&
                                   parsedDate.Date == selectedColabAtual.DayNumberFormatted.Date);

            if (diaColabAtual == null)
            {
                await DisplayAlert("Erro",
                    $"Dia {selectedColabAtual.DayNumberFormatted.ToShortDateString()} não encontrado para {nomeabreviado}.",
                    "OK");
                return;
            }

            if (string.IsNullOrEmpty(diaColabAtual.idPMTColabDia) ||
                !int.TryParse(diaColabAtual.idPMTColabDia, out int idPMTColabDiaColabAtual))
            {
                await DisplayAlert("Erro",
                    $"ID inválido para o dia selecionado de {nomeabreviado}.",
                    "OK");
                return;
            }

            // REPETIR O MESMO PROCESSO PARA O COLABORADOR DE TROCA
            Debug.WriteLine($"Obtendo dias para {nomecolabtroca} (ID: {IdColabTroca})");
            var getDiasColabTrocaResponse = await _service.GetDiasAsync(IdPMT, 0, IdColabTroca, Token);

            // [Adicionar aqui o mesmo tratamento de erro feito para o colaborador atual]

            // Obter turnos - COM TRATAMENTO MELHORADO
            Debug.WriteLine($"Obtendo turnos para {nomeabreviado}");
            var turnosResponse = await _service.GetTurnosAsync(idPMTColabDiaColabAtual, IdColaborador, Token);

            if (turnosResponse?.Body?.GetTurnosResult == null)
            {
                await DisplayAlert("Erro", "Resposta inválida ao obter turnos.", "OK");
                return;
            }

            if (turnosResponse.Body.GetTurnosResult._erro?.erro != 0)
            {
                string errorMsg = turnosResponse.Body.GetTurnosResult._erro?.erroMensagem ??
                                "Erro desconhecido ao obter turnos";
                await DisplayAlert("Erro", errorMsg, "OK");
                return;
            }

            // Restante do código para processar a troca...

            // [Manter o restante da lógica original]
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro inesperado: {ex}");
            await DisplayAlert("Erro", $"Ocorreu um erro inesperado: {ex.Message}", "OK");
        }
    }

    private void OnColabAtualImageTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is TrocaDiaItem item && item.IsSelectable)
        {
            if (item.IsColabAtualSelected)
            {
                item.IsColabAtualSelected = false;
                _selectedColabAtualItem = null;
            }
            else
            {
                if (_selectedColabAtualItem != null)
                {
                    _selectedColabAtualItem.IsColabAtualSelected = false;
                }

                item.IsColabAtualSelected = true;
                _selectedColabAtualItem = item;
            }
        }
    }

    private void OnColab2ImageTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is TrocaDiaItem item && item.IsSelectable)
        {
            if (item.IsColab2Selected)
            {
                item.IsColab2Selected = false;
                _selectedColab2Item = null;
            }
            else
            {
                if (_selectedColab2Item != null)
                {
                    _selectedColab2Item.IsColab2Selected = false;
                }

                item.IsColab2Selected = true;
                _selectedColab2Item = item;
            }
        }
    }

    public TrocaDiaItem GetSelectedColabAtualItem()
    {
        return _selectedColabAtualItem;
    }

    public TrocaDiaItem GetSelectedColab2Item()
    {
        return _selectedColab2Item;
    }

    public void ClearAllSelections()
    {
        if (_selectedColabAtualItem != null)
        {
            _selectedColabAtualItem.IsColabAtualSelected = false;
            _selectedColabAtualItem = null;
        }

        if (_selectedColab2Item != null)
        {
            _selectedColab2Item.IsColab2Selected = false;
            _selectedColab2Item = null;
        }
    }
}