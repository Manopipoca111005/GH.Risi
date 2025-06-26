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
    public const string OpcaoDefinicoes = "Definições";
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
                    IsSelectable = date.Date > DateTime.Today.Date,
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

    private async void OnAnteriorClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AdicionarTrocaPasso2(IdColaborador, Token, IdPMT, nomeabreviado, Servico));
    }

    private async void OnSeguinteClicked(object sender, EventArgs e)
    {
        var selectedColabAtual = GetSelectedColabAtualItem();
        var selectedColab2 = GetSelectedColab2Item();

        if (selectedColabAtual == null || selectedColab2 == null)
        {
            await DisplayAlert("Atenção", "Por favor, selecione um dia para o colaborador atual e um dia para o colaborador da troca.", "OK");
            return;
        }

        try
        {
            int dia1Formatted = int.Parse(selectedColabAtual.DayNumberFormatted.ToString("yyyyMMdd"));
            int dia2Formatted = int.Parse(selectedColab2.DayNumberFormatted.ToString("yyyyMMdd"));

            string idPMTColabDiaColabAtualStr = string.Empty;
            int idPMTColabDiaColabAtual = 0;
            var getDiasColabAtualResponse = await _service.GetDiasAsync(IdPMT, idPMTColabDiaColabAtual, IdColaborador, Token);
            if (getDiasColabAtualResponse?.Body?.GetDiasResult?.aDias != null)
            {
                var diaColabAtual = getDiasColabAtualResponse.Body.GetDiasResult.aDias.FirstOrDefault(d => DateTime.TryParse(d.data, out DateTime parsedDate) && parsedDate.Date == selectedColabAtual.DayNumberFormatted.Date);
                if (diaColabAtual != null && int.TryParse(diaColabAtual.idPMTColabDia, out idPMTColabDiaColabAtual))
                {
                    idPMTColabDiaColabAtualStr = diaColabAtual.idPMTColabDia;
                }
            }


            string idPMTColabDiaColabTrocaStr = string.Empty;
            int idPMTColabDiaColabTroca = 0;
            var getDiasColabTrocaResponse = await _service.GetDiasAsync(IdPMT, idPMTColabDiaColabTroca, IdColabTroca, Token);
            if (getDiasColabTrocaResponse?.Body?.GetDiasResult?.aDias != null)
            {
                var diaColabTroca = getDiasColabTrocaResponse.Body.GetDiasResult.aDias.FirstOrDefault(d => DateTime.TryParse(d.data, out DateTime parsedDate) && parsedDate.Date == selectedColab2.DayNumberFormatted.Date);
                if (diaColabTroca != null && int.TryParse(diaColabTroca.idPMTColabDia, out idPMTColabDiaColabTroca))
                {
                    idPMTColabDiaColabTrocaStr = diaColabTroca.idPMTColabDia;
                }
            }
            
            var turnos = await _service.GetTurnosAsync(idPMTColabDiaColabAtual, IdColaborador, Token);

            var turnostroca = await _service.GetTurnosAsync(idPMTColabDiaColabTroca, IdColabTroca, Token);

            var trocasTurnosResponse = await _service.GetTrocasTurnosAsync(
                IdColaborador,
                Token,
                IdPMT,
                IdColabTroca,
                (short)dia1Formatted,
                (short)dia2Formatted
            );


            DateTime dataParaExibir1 = DateTime.ParseExact(dia1Formatted.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
            DateTime dataParaExibir2 = DateTime.ParseExact(dia2Formatted.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);

            string dia1Exibicao = dataParaExibir1.ToString("d", new CultureInfo("pt-PT"));
            string dia2Exibicao = dataParaExibir2.ToString("d", new CultureInfo("pt-PT"));

            string colabAtualSiglaParaPassar = selectedColabAtual.colabatualsigla;
            string colab2SiglaParaPassar = selectedColab2.ColabselecionadoSigla;

            int turno1 = turnos?.Body?.GetTurnosResult?.aTurnos?.FirstOrDefault(t => t.sigla == selectedColabAtual.colabatualsigla || t.descTurno == selectedColabAtual.colabatualsigla)?.idTurno ?? -1;
            int turno2 = turnostroca?.Body?.GetTurnosResult?.aTurnos?.FirstOrDefault(t => t.sigla == selectedColab2.ColabselecionadoSigla || t.descTurno == selectedColab2.ColabselecionadoSigla)?.idTurno ?? -1;

            if (trocasTurnosResponse?.Body?.GetTrocasTurnosResult?._erro?.erro == 0)
            {
                await Navigation.PushAsync(new AdicionarTrocasPasso4(IdColaborador, Token, IdPMT, IdColabTroca, nomeabreviado, nomecolabtroca, dia1Exibicao, dia2Exibicao, colabAtualSiglaParaPassar, colab2SiglaParaPassar, Servico, turno1, turno2));
            }
            else
            {
                string mensagemErro = trocasTurnosResponse?.Body?.GetTrocasTurnosResult?._erro?.erroMensagem ?? "Erro desconhecido ao obter trocas de turnos.";
                await DisplayAlert("Erro", $"Ocorreu um erro: {mensagemErro}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro: {ex.Message}", "OK");
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
