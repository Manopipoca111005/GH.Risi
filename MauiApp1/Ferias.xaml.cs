using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using GH_Metodos;
using Syncfusion.Maui.Core.Hosting;

namespace MauiApp1;

public partial class Ferias : ContentPage, INotifyPropertyChanged
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    private readonly int _idcolaborador;
    private readonly string _token;

    public const string OpcaoDefinicoes = "Definições";
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";

    private readonly Dictionary<int, string> _monthIcons = new Dictionary<int, string>
    {
        { 1, "janeiro.png" }, { 2, "fevereiro.png" }, { 3, "marco.png" }, { 4, "abril.png" },
        { 5, "maio.png" }, { 6, "junho.png" }, { 7, "julho.png" }, { 8, "agosto.png" },
        { 9, "setembro.png" }, { 10, "outubro.png" }, { 11, "novembro.png" }, { 12, "dezembro.png" }
    };

    private List<DateTime> _currentYearVacationDates = new List<DateTime>();

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

    public Ferias(int idcolaborador, string token)
    {
        InitializeComponent();
        _idcolaborador = idcolaborador;
        _token = token;

        Years = new ObservableCollection<short>();
        VacationMonths = new ObservableCollection<VacationMonth>();
        CalendarDays = new ObservableCollection<CalendarDay>();
        SummaryItems = new ObservableCollection<SummaryItem>();
        editarferias.IsVisible = false;

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadInitialData();
    }

    private async Task LoadInitialData()
    {
        var resumoResult = await _service.GetFeriasResumoAsync(_idcolaborador, _token);
        if (resumoResult?.Body?.GetFeriasResumoResult?._erro?.erro == 0 && resumoResult.Body.GetFeriasResumoResult.aFeriasResumoAno.Any())
        {
            var years = resumoResult.Body.GetFeriasResumoResult.aFeriasResumoAno.Select(r => r.ano).OrderByDescending(y => y);
            foreach (var year in years)
            {
                Years.Add(year);
            }

            if (Years.Any())
            {
                SelectedYear = Years.FirstOrDefault();
            }
        }
    }

    private async void LoadYearData()
    {
        if (SelectedYear == 0) return;

        await LoadSummaryItems();
        await LoadVacationDaysAndCalendar();
    }

    private async Task LoadSummaryItems()
    {
        var resumoResult = await _service.GetFeriasResumoAsync(_idcolaborador, _token);
        var yearSummary = resumoResult?.Body?.GetFeriasResumoResult?.aFeriasResumoAno.FirstOrDefault(r => r.ano == SelectedYear);

        if (yearSummary != null)
        {
            SummaryItems.Clear();
            SummaryItems.Add(new SummaryItem { Description = "Disponíveis ano atual", Value = yearSummary.diasFerias.ToString() });
            SummaryItems.Add(new SummaryItem { Description = "Disponíveis ano anterior", Value = yearSummary.diasFeriasAnteriores.ToString() });
            SummaryItems.Add(new SummaryItem { Description = "Artºs goz. ano ant. p/ano atual", Value = yearSummary.diasGozadosArt66.ToString() });
            SummaryItems.Add(new SummaryItem { Description = "Artºs goz. ano atual", Value = "0" });
            SummaryItems.Add(new SummaryItem { Description = "Marcados", Value = yearSummary.diasMarcados.ToString() });
            SummaryItems.Add(new SummaryItem { Description = "Por Marcar", Value = yearSummary.diasPorMarcar.ToString() });
            SummaryItems.Add(new SummaryItem { Description = "Gozados", Value = yearSummary.diasGozados.ToString() });
            SummaryItems.Add(new SummaryItem { Description = "Por Gozar", Value = yearSummary.diasPorGozar.ToString() });
        }
    }

    private async Task LoadVacationDaysAndCalendar()
    {
        var diasResult = await _service.GetFeriasDiasAsync(_idcolaborador, _token, SelectedYear);
        if (diasResult?.Body?.GetFeriasDiasResult?._erro?.erro == 0)
        {
            _currentYearVacationDates = diasResult.Body.GetFeriasDiasResult.aDatas.ToList();

            if (_currentYearVacationDates.Any())
            {
                CalendarContainer.IsVisible = true;
                NoVacationMessageLabel.IsVisible = false;

                UpdateVacationMonths(_currentYearVacationDates);

                int monthToShow = _currentYearVacationDates.OrderBy(d => d.Month).Last().Month;
                GenerateCalendar(SelectedYear, monthToShow, _currentYearVacationDates);
            }
            else
            {
                CalendarContainer.IsVisible = false;
                NoVacationMessageLabel.IsVisible = true;
            }
        }
        else
        {
            CalendarContainer.IsVisible = false;
            NoVacationMessageLabel.IsVisible = true;
        }
    }

    private void UpdateVacationMonths(List<DateTime> vacationDates)
    {
        VacationMonths.Clear();
        var monthGroups = vacationDates.GroupBy(d => d.Month)
                                      .Select(g => new { Month = g.Key, Count = g.Count() })
                                      .OrderBy(x => x.Month);

        foreach (var group in monthGroups)
        {
            VacationMonths.Add(new VacationMonth
            {
                MonthNumber = group.Month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(group.Month),
                Days = group.Count,
                Icon = _monthIcons.GetValueOrDefault(group.Month, "default_icon.png")
            });
        }
    }

    private void GenerateCalendar(int year, int month, List<DateTime> vacationDates)
    {
        CalendarDays.Clear();
        var culture = new CultureInfo("pt-PT");
        CalendarTitle = $"{culture.DateTimeFormat.GetMonthName(month).ToUpper()} {year}";

        DateTime firstDayOfMonth = new DateTime(year, month, 1);
        int firstDayOfWeek = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;
        int daysInMonth = DateTime.DaysInMonth(year, month);

        for (int i = 0; i < firstDayOfWeek; i++)
        {
            CalendarDays.Add(new CalendarDay { Day = "" });
        }

        for (int day = 1; day <= daysInMonth; day++)
        {
            var currentDate = new DateTime(year, month, day);
            bool isVacation = vacationDates.Any(d => d.Date == currentDate.Date);

            CalendarDays.Add(new CalendarDay
            {
                Day = day.ToString(),
                BackgroundColor = isVacation ? Colors.DodgerBlue : Colors.Transparent,
                TextColor = isVacation ? Colors.White : Colors.Black
            });

        }
    }

    private void YearComboBox_SelectionChanged(object sender, EventArgs e)
    {
        if (YearComboBox.SelectedItem is short year)
        {
            SelectedYear = year;
        }
        var data = DateTime.Now.Year;
        if (_selectedYear == data)
        {
            editarferias.IsVisible = true;
        }
        else
        {
            editarferias.IsVisible = false;
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        string acao = await DisplayActionSheet(
            "Menu de Opções",
            OpcaoCancelar,
            null,
            OpcaoDefinicoes,
            OpcaoSair);

        if (!string.IsNullOrEmpty(acao))
        {
            switch (acao)
            {
                case OpcaoDefinicoes:
                    await Navigation.PushAsync(new ContentPage());
                    break;
                case OpcaoSair:
                    await Navigation.PushAsync(new ContentPage());
                    break;
                case OpcaoCancelar:
                    break;
            }
        }
    }

    private void MonthIcon_Tapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is VacationMonth tappedMonth)
        {
            GenerateCalendar(SelectedYear, tappedMonth.MonthNumber, _currentYearVacationDates);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new AlterarFerias(_idcolaborador, _token));
    }
}