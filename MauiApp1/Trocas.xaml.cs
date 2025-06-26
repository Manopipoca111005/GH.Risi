using GH_Metodos;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Syncfusion.Maui.Inputs;
using System.ComponentModel;

namespace MauiApp1;

public partial class Trocas : ContentPage
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    public const string OpcaoDefinicoes = "Definições";
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";
    public readonly int IdColaborador;
    public readonly string Token;
    private int _currentFilter = -1;
    public string nomeabreviado;
    public int idpmt;

    private bool _isPickerInitialized = false;
    private bool _preventInitialDropdown = true;

    public Trocas(int idcolaborador, string token, string nome_avreviado)
    {
        InitializeComponent();
        IdColaborador = idcolaborador;
        Token = token;
        nomeabreviado = nome_avreviado;
        BindingContext = this;
        UpdateButtonStyles(EmAbertoText);

        AnoPicker.IsEnabled = false;
        MesPicker.IsEnabled = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_isPickerInitialized)
        {
            await Task.Delay(100);

            InitializePickers();

            AnoPicker.IsEnabled = true;
            MesPicker.IsEnabled = true;

            await LoadDataFromWebService();
            _isPickerInitialized = true;
            _preventInitialDropdown = false;
        }
    }

    private void InitializePickers()
    {
        var anos = Enumerable.Range(DateTime.Now.Year - 5, 10).Select(a => a.ToString()).ToList(); 
        AnoPicker.ItemsSource = anos;
        AnoPicker.SelectedItem = DateTime.Now.Year.ToString(); 

        var meses = DateTimeFormatInfo.CurrentInfo.MonthNames
            .Where(m => !string.IsNullOrEmpty(m))
            .Select((m, i) => new { Nome = m, Numero = i + 1 })
            .ToList();

        MesPicker.ItemsSource = meses;
        MesPicker.DisplayMemberPath = "Nome";
        MesPicker.SelectedItem = meses.FirstOrDefault(m => m.Numero == DateTime.Now.Month);

        
    }

    private void UpdateButtonStyles(Button selectedButton)
    {
        var buttons = new List<Button> { EmAbertoText, AceitesText, NaoAceitesText };

        foreach (var btn in buttons)
        {
            btn.BackgroundColor = Colors.White;
            btn.TextColor = Color.FromArgb("#00AEEF");
            btn.BorderColor = Color.FromArgb("#00AEEF");
        }

        selectedButton.BackgroundColor = Color.FromArgb("#00AEEF");
        selectedButton.TextColor = Colors.White;
        selectedButton.BorderColor = Colors.Transparent;
    }

    private async void EmAberto_Clicked(object sender, EventArgs e)
    {
        _currentFilter = -1;
        UpdateButtonStyles((Button)sender);
        await LoadDataFromWebService();
    }

    private async void Aceites_Clicked(object sender, EventArgs e)
    {
        _currentFilter = 1;
        UpdateButtonStyles((Button)sender);
        await LoadDataFromWebService();
    }

    private async void NaoAceites_Clicked(object sender, EventArgs e)
    {
        _currentFilter = 0;
        UpdateButtonStyles((Button)sender);
        await LoadDataFromWebService();
    }

    private async Task LoadDataFromWebService()
    {
        try
        {
            var mesSelecionado = MesPicker.SelectedItem as dynamic;
            string anoSelecionadoString = AnoPicker.SelectedItem?.ToString();
            short anoSelecionado = 0; 

            if (mesSelecionado == null || string.IsNullOrEmpty(anoSelecionadoString) || !short.TryParse(anoSelecionadoString, out anoSelecionado))
            {
                return;
            }
            byte numeroMes = (byte)mesSelecionado.Numero;
            

            var response = await _service.GetTrocasColaboradorAsync(IdColaborador, Token, _currentFilter, anoSelecionado, numeroMes);
            var turnos = response?.Body?.GetTrocasColaboradorResult?.aTrocas;

            StackTrocas.Children.Clear();
            if (turnos == null || turnos.Length == 0)
            {
                lblRegistoText.IsVisible = true;
                return;
            }
            lblRegistoText.IsVisible = false;

            var groupedTrocas = turnos
                .GroupBy(t => new { t.dia, t.descServico })
                .OrderBy(g => g.Key.dia);

            foreach (var group in groupedTrocas)
            {
                var dateOnly = new DateOnly(anoSelecionado, numeroMes, group.Key.dia);
                string headerDateText = dateOnly.ToString("dd MMM").ToUpper().Replace(".", "");

                string serviceDescription = group.Key.descServico?.Replace("Serviço ", "").Trim();
                string headerServiceText = $"SERVIÇO {serviceDescription}";

                var groupHeaderMainGrid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Star }
                    },
                    RowDefinitions = new RowDefinitionCollection
                    {
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto }
                    },
                    BackgroundColor = Colors.White,
                    Padding = new Thickness(10, 0, 0, 0),
                    RowSpacing = 0,
                    ColumnSpacing = 0,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                var serviceLabel = new Label
                {
                    Text = headerServiceText,
                    Style = (Style)Resources["GroupHeader"],
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start,
                    Padding = new Thickness(0, 5, 0, 0)
                };
                Grid.SetRow(serviceLabel, 0);
                Grid.SetColumn(serviceLabel, 0);
                Grid.SetColumnSpan(serviceLabel, 4);
                groupHeaderMainGrid.Children.Add(serviceLabel);

                var thinBlueLine = new BoxView
                {
                    HeightRequest = 2,
                    BackgroundColor = Color.FromArgb("#ADD8E6"),
                    Margin = new Thickness(0, 0, 10, 0)
                };
                Grid.SetRow(thinBlueLine, 1);
                Grid.SetColumn(thinBlueLine, 0);
                Grid.SetColumnSpan(thinBlueLine, 4);
                groupHeaderMainGrid.Children.Add(thinBlueLine);

                var dateLabel = new Label
                {
                    Text = headerDateText,
                    Style = (Style)Resources["GroupHeader"],
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start,
                    FontSize = 14,
                    TextColor = Colors.Black,
                    Padding = new Thickness(0, 0, 0, 5)
                };
                Grid.SetRow(dateLabel, 2);
                Grid.SetColumn(dateLabel, 0);
                groupHeaderMainGrid.Children.Add(dateLabel);

                var blueHeaderGrid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Star }
                    },
                    BackgroundColor = Color.FromArgb("#00AEEF"),
                    Padding = new Thickness(5, 0, 5, 0),
                    HeightRequest = 40,
                    VerticalOptions = LayoutOptions.Fill,
                    HorizontalOptions = LayoutOptions.Fill,
                    Margin = new Thickness(0)
                };

                var labelPrevHeader = new Label { Text = "PREV.", Style = (Style)Resources["BlueTableHeader"] };
                blueHeaderGrid.Children.Add(labelPrevHeader);
                Grid.SetColumn(labelPrevHeader, 0);

                var labelPropHeader = new Label { Text = "PROP.", Style = (Style)Resources["BlueTableHeader"] };
                blueHeaderGrid.Children.Add(labelPropHeader);
                Grid.SetColumn(labelPropHeader, 1);

                var labelConfirmoHeader = new Label { Text = "CONFIRMO?", Style = (Style)Resources["BlueTableHeader"] };
                blueHeaderGrid.Children.Add(labelConfirmoHeader);
                Grid.SetColumn(labelConfirmoHeader, 2);

                Grid.SetRow(blueHeaderGrid, 2);
                Grid.SetColumn(blueHeaderGrid, 1);
                Grid.SetColumnSpan(blueHeaderGrid, 3);
                groupHeaderMainGrid.Children.Add(blueHeaderGrid);

                StackTrocas.Children.Add(groupHeaderMainGrid);

                foreach (var troca in group)
                {
                    var dataGrid = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition { Width = new GridLength(1.5, GridUnitType.Star) },
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = GridLength.Star }
                        },
                        BackgroundColor = Colors.White,
                        Padding = new Thickness(10, 0, 10, 0),
                        RowSpacing = 0,
                        ColumnSpacing = 5
                    };

                    dataGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    dataGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Absolute) });
                    dataGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    var (labelColab1, labelPrev1, labelProp1, labelConfirmo1) = CreateTrocaRowViews(troca.nomeAbreviado1, troca.previsto1, troca.proposto1, troca.confirmada1);
                    Grid.SetRow(labelColab1, 0); Grid.SetColumn(labelColab1, 0);
                    Grid.SetRow(labelPrev1, 0); Grid.SetColumn(labelPrev1, 1);
                    Grid.SetRow(labelProp1, 0); Grid.SetColumn(labelProp1, 2);
                    Grid.SetRow(labelConfirmo1, 0); Grid.SetColumn(labelConfirmo1, 3);
                    dataGrid.Children.Add(labelColab1);
                    dataGrid.Children.Add(labelPrev1);
                    dataGrid.Children.Add(labelProp1);
                    dataGrid.Children.Add(labelConfirmo1);

                    var internalDivider = new BoxView
                    {
                        HeightRequest = 1,
                        BackgroundColor = Color.FromHex("#F0F0F0")
                    };
                    Grid.SetRow(internalDivider, 1);
                    Grid.SetColumn(internalDivider, 0);
                    Grid.SetColumnSpan(internalDivider, 4);
                    dataGrid.Children.Add(internalDivider);

                    var (labelColab2, labelPrev2, labelProp2, labelConfirmo2) = CreateTrocaRowViews(troca.nomeAbreviado2, troca.previsto2, troca.proposto2, troca.confirmada2);
                    Grid.SetRow(labelColab2, 2); Grid.SetColumn(labelColab2, 0);
                    Grid.SetRow(labelPrev2, 2); Grid.SetColumn(labelPrev2, 1);
                    Grid.SetRow(labelProp2, 2); Grid.SetColumn(labelProp2, 2);
                    Grid.SetRow(labelConfirmo2, 2); Grid.SetColumn(labelConfirmo2, 3);
                    dataGrid.Children.Add(labelColab2);
                    dataGrid.Children.Add(labelPrev2);
                    dataGrid.Children.Add(labelProp2);
                    dataGrid.Children.Add(labelConfirmo2);

                    StackTrocas.Children.Add(dataGrid);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exceção: {ex.Message}");
            await DisplayAlert("Erro na Aplicação", $"Falha ao carregar trocas: {ex.Message}", "OK");
        }
    }

    private (Label labelColab, Label labelPrev, Label labelProp, Label labelConfirmo) CreateTrocaRowViews(string nomeAbreviado, string previsto, string proposto, int? confirmada)
    {
        var labelColab = new Label
        {
            Text = $"Colab. {nomeAbreviado.Replace("Colab. ", "")}",
            Style = (Style)Resources["TableCell"],
            Padding = new Thickness(0, 0, 0, 0),
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Start
        };

        string prevText = previsto?.Split(';')[0].Trim() ?? "";
        var labelPrev = new Label
        {
            Text = prevText,
            Style = (Style)Resources["TableCell"],
            Padding = new Thickness(5),
            VerticalOptions = LayoutOptions.Center
        };

        string propText = proposto?.Split(';')[0].Trim() ?? "";
        var labelProp = new Label
        {
            Text = propText,
            Style = (Style)Resources["TableCell"],
            Padding = new Thickness(5),
            VerticalOptions = LayoutOptions.Center
        };

        var labelConfirmo = new Label
        {
            FontAttributes = FontAttributes.Bold,
            Style = (Style)Resources["TableCell"],
            Padding = new Thickness(2),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };

        if (confirmada == 0)
        {
            labelConfirmo.Text = "";
        }
        else if (confirmada == -1)
        {
            labelConfirmo.Text = "?";
            labelConfirmo.TextColor = Color.FromArgb("#EFAF00");
        }
        else if (confirmada == 1)
        {
            labelConfirmo.Text = "Confirmada";
            labelConfirmo.TextColor = Colors.Green;
        }
        else
        {
            labelConfirmo.Text = "";
        }

        return (labelColab, labelPrev, labelProp, labelConfirmo);
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
            }
        }
    }

    private async void SearchIcon_Tapped(object sender, EventArgs e)
    {
        await LoadDataFromWebService();
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new AdicionarTroca(IdColaborador, Token, idpmt, nomeabreviado));
    }
}