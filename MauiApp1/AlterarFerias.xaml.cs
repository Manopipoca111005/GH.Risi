using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using GH_Metodos;

namespace MauiApp1;

public partial class AlterarFerias : ContentPage, INotifyPropertyChanged
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    private readonly int _idcolaborador;
    private readonly string _token;

    private List<DateTime> _vacationDates = new List<DateTime>();
    private List<DateTime> _blockedDates = new List<DateTime>();
    private List<DateTime> _newlySelectedDates = new List<DateTime>();
    private bool _allowWeekendSelection = false;
    private int _currentMonthDisplayed;

    private readonly Dictionary<int, string> _monthIcons = new Dictionary<int, string>
    {
        { 1, "janeiro.png" }, { 2, "fevereiro.png" }, { 3, "marco.png" }, { 4, "abril.png" },
        { 5, "maio.png" }, { 6, "junho.png" }, { 7, "julho.png" }, { 8, "agosto.png" },
        { 9, "setembro.png" }, { 10, "outubro.png" }, { 11, "novembro.png" }, { 12, "dezembro.png" }
    };

    public ObservableCollection<short> Years { get; set; }
    public ObservableCollection<VacationMonth> VacationMonths { get; set; }
    public ObservableCollection<CalendarDay> CalendarDays { get; set; }
    public ObservableCollection<SummaryItem> SummaryItems { get; set; }

    private short _selectedYear;
    public short SelectedYear
    {
        get => _selectedYear;
        set
        {
            if (_selectedYear != value)
            {
                _selectedYear = value;
                OnPropertyChanged(nameof(SelectedYear));
                LoadYearData();
            }
        }
    }

    private string _calendarTitle;
    public string CalendarTitle
    {
        get => _calendarTitle;
        set
        {
            _calendarTitle = value;
            OnPropertyChanged(nameof(CalendarTitle));
        }
    }

    public AlterarFerias(int idcolaborador, string token)
    {
        InitializeComponent();
        _idcolaborador = idcolaborador;
        _token = token;

        Years = new ObservableCollection<short>();
        VacationMonths = new ObservableCollection<VacationMonth>();
        CalendarDays = new ObservableCollection<CalendarDay>();
        SummaryItems = new ObservableCollection<SummaryItem>();

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadInitialData();
    }

    private async Task LoadInitialData()
    {
        var currentYear = (short)DateTime.Now.Year;
        Years.Clear();
        Years.Add(currentYear);
        SelectedYear = currentYear;
    }

    private async void LoadYearData()
    {
        if (SelectedYear == 0) return;
        _currentMonthDisplayed = DateTime.Now.Month;
        await LoadCalendarData();
        await LoadSummaryItems();
    }

    private async Task LoadCalendarData()
    {
        CalendarContainer.IsVisible = true;
        NoVacationMessageLabel.IsVisible = false;

        var result = await _service.GetColaboradorFeriasAsync(_idcolaborador, _token, SelectedYear);
        if (result?.Body?.GetColaboradorFeriasResult?._erro?.erro == 0)
        {
            var data = result.Body.GetColaboradorFeriasResult;
            _vacationDates = data.aDiasMarcados?.ToList() ?? new List<DateTime>();
            _blockedDates = data.aDiasBloqueados?.ToList() ?? new List<DateTime>();
            _allowWeekendSelection = data.permitirFeriasFDS;
            _newlySelectedDates.Clear();
        }
        else
        {
            await DisplayAlert("Erro", "Não foi possível carregar os dados do calendário.", "OK");
        }

        UpdateVacationMonths();
        GenerateCalendar();
    }

    private async Task LoadSummaryItems()
    {
        var resumoResult = await _service.GetFeriasResumoAsync(_idcolaborador, _token);
        var yearSummary = resumoResult?.Body?.GetFeriasResumoResult?.aFeriasResumoAno.FirstOrDefault(r => r.ano == SelectedYear);

        if (yearSummary != null)
        {
            SummaryItems.Clear();
            SummaryItems.Add(new SummaryItem { Description = "Marcados", Value = yearSummary.diasMarcados.ToString() });
            SummaryItems.Add(new SummaryItem { Description = "Por Marcar", Value = yearSummary.diasPorMarcar.ToString() });
        }
    }

    private void UpdateVacationMonths()
    {
        VacationMonths.Clear();
        var culture = new CultureInfo("pt-PT");
        var allMarkedDays = _vacationDates.Concat(_newlySelectedDates).ToList();
        var monthCounts = allMarkedDays.GroupBy(d => d.Month)
                                       .ToDictionary(g => g.Key, g => g.Count());

        for (int i = 1; i <= 12; i++)
        {
            VacationMonths.Add(new VacationMonth
            {
                MonthNumber = i,
                MonthName = culture.DateTimeFormat.GetMonthName(i),
                Days = monthCounts.GetValueOrDefault(i, 0),
                Icon = _monthIcons.GetValueOrDefault(i, "default_icon.png")
            });
        }
    }

    private void GenerateCalendar()
    {
        CalendarDays.Clear();
        var culture = new CultureInfo("pt-PT");
        CalendarTitle = $"{culture.DateTimeFormat.GetMonthName(_currentMonthDisplayed).ToUpper()} {SelectedYear}";

        DateTime firstDayOfMonth = new DateTime(SelectedYear, _currentMonthDisplayed, 1);
        int daysInMonth = DateTime.DaysInMonth(SelectedYear, _currentMonthDisplayed);

        for (int i = 0; i < ((int)firstDayOfMonth.DayOfWeek + 6) % 7; i++)
        {
            CalendarDays.Add(new CalendarDay { Day = "" });
        }

        for (int day = 1; day <= daysInMonth; day++)
        {
            var currentDate = new DateTime(SelectedYear, _currentMonthDisplayed, day);
            var dayModel = new CalendarDay { Day = day.ToString(), FullDate = currentDate };

            bool isWeekend = currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday;
            bool isBlocked = _blockedDates.Any(d => d.Date == currentDate.Date);
            bool isVacation = _vacationDates.Any(d => d.Date == currentDate.Date);
            bool isSelected = _newlySelectedDates.Any(d => d.Date == currentDate.Date);

            dayModel.IsSelectable = !isBlocked && !isVacation && (!isWeekend || _allowWeekendSelection);

            if (isVacation)
            {
                dayModel.BackgroundColor = Colors.DodgerBlue;
                dayModel.TextColor = Colors.White;
            }
            else if (isSelected)
            {
                dayModel.BackgroundColor = Colors.DeepSkyBlue;
                dayModel.TextColor = Colors.White;
            }
            else if (!dayModel.IsSelectable)
            {
                dayModel.BackgroundColor = Colors.LightGray;
                dayModel.TextColor = Colors.DarkGray;
            }

            CalendarDays.Add(dayModel);
        }
    }

    private void CalendarDay_Tapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not CalendarDay day || string.IsNullOrEmpty(day.Day) || !day.IsSelectable)
        {
            return;
        }

        var date = day.FullDate;
        if (_newlySelectedDates.Any(d => d.Date == date.Date))
        {
            _newlySelectedDates.RemoveAll(d => d.Date == date.Date);
        }
        else
        {
            _newlySelectedDates.Add(date);
        }

        GenerateCalendar();
        UpdateVacationMonths();
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        if (!_newlySelectedDates.Any())
        {
            await DisplayAlert("Aviso", "Nenhum dia novo foi selecionado.", "OK");
            return;
        }

        // ALTERAÇÃO FINAL: Substituir o separador ";" por ",".
        var diasPropostos = string.Join(",", _newlySelectedDates.OrderBy(d => d.Date).Select(d => d.ToString("yyyy-MM-dd")));

        int idMotivoAlteracaoFerias = 1;

        var result = await _service.SetPropostaPlanoFeriasAsync(_idcolaborador, _token, SelectedYear, idMotivoAlteracaoFerias, diasPropostos);

        if (result?.Body?.SetPropostaPlanoFeriasResult?.erro == 0)
        {
            await DisplayAlert("Sucesso", "A sua proposta de férias foi enviada com sucesso.", "OK");
            await LoadCalendarData();
        }
        else
        {
            var errorMessage = result?.Body?.SetPropostaPlanoFeriasResult?.erroMensagem ?? "Ocorreu um erro desconhecido.";
            await DisplayAlert("Erro", $"Não foi possível guardar a proposta.\n\nErro: {errorMessage}", "OK");
        }
    }

    private void YearComboBox_SelectionChanged(object sender, EventArgs e)
    {
        if (YearComboBox.SelectedItem is short year && _selectedYear != year)
        {
            SelectedYear = year;
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        string acao = await DisplayActionSheet(
           "Menu de Opções",
           "Cancelar",
           null,
           "Definições",
           "Sair da app");

        if (!string.IsNullOrEmpty(acao))
        {
            switch (acao)
            {
                case "Definições":
                    await Navigation.PushAsync(new ContentPage());
                    break;
                case "Sair da app":
                    await Navigation.PushAsync(new ContentPage());
                    break;
            }
        }
    }

    private void MonthIcon_Tapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is VacationMonth tappedMonth)
        {
            _currentMonthDisplayed = tappedMonth.MonthNumber;
            GenerateCalendar();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    
}


public class CalendarDay
{
    public string Day { get; set; }
    public Color BackgroundColor { get; set; } = Colors.Transparent;
    public Color TextColor { get; set; } = Colors.Black;
    public DateTime FullDate { get; set; }
    public bool IsSelectable { get; set; }
}

public class VacationMonth
{
    public int MonthNumber { get; set; }
    public string MonthName { get; set; }
    public int Days { get; set; }
    public string Icon { get; set; }
}

public class SummaryItem
{
    public string Description { get; set; }
    public string Value { get; set; }
}