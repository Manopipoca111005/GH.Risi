using GH_Metodos;
namespace MauiApp1;
using Microsoft.Maui.Controls;
using System.Globalization;

public partial class PMTsDetalhesTurnos : ContentPage
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    public const string OpcaoDefinicoes = "Definições";
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";
    private int IdColaborador;
    private readonly string Token;
    private readonly string mesano;
    private string nome_abreviado;
    private readonly DateTime DataSelecionada;
    private readonly int IdPMT;
    private bool isOutrosColaboradoresVisible = false;

    public PMTsDetalhesTurnos(int id_colaborador, string token, DateTime dataselecionada, string NomeAbreviado, int idpmt)
    {
        InitializeComponent();
        IdColaborador = id_colaborador;
        Token = token;
        nome_abreviado = NomeAbreviado;
        colab.Text = nome_abreviado;
        DataSelecionada = dataselecionada;
        IdPMT = idpmt;

        var cultura = new CultureInfo("pt-PT");
        MonthLabel.Text = DataSelecionada.ToString("MMM", cultura).ToUpper();
        DayLabel.Text = DataSelecionada.ToString("dd");
        DayOfWeekLabel.Text = DataSelecionada.ToString("dddd", cultura);

        CarregarPMTsAsync();
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

    private string GetFullShiftTitle(string originalTitle)
    {
        if (originalTitle?.Trim().ToUpper() == "DS")
        {
            return "Descanso Semanal";
        }
        return originalTitle;
    }

    private Grid CreateShiftListItemView(string colaboradorNome, string turnoTituloOriginal, string turnoDataHoraInicio, string turnoDataHoraFim, bool isPrincipalView)
    {
        string fullShiftTitleForDetails = GetFullShiftTitle(turnoTituloOriginal);
        string textOnImage = !string.IsNullOrEmpty(turnoTituloOriginal) ? turnoTituloOriginal.Split(' ')[0] : "";

        var listItem = new Grid
        {
        };
        listItem.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        listItem.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        var imagemGridNovo = new Grid
        {
            VerticalOptions = LayoutOptions.Center
        };
        var imagemFile = new Image
        {
            Source = "turnos.png",
        };
        var labelDentroSkewed = new Label
        {
            Text = textOnImage,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        new BoxView
        {
            HeightRequest = 2,
            BackgroundColor = Color.FromArgb("#00A2C7"),
        };
        imagemGridNovo.Children.Add(imagemFile);
        imagemGridNovo.Children.Add(labelDentroSkewed);
        Grid.SetColumn(imagemGridNovo, 0);
        listItem.Children.Add(imagemGridNovo);

        var detailsLayout = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.Center
        };

        if (!isPrincipalView)
        {
            var colabInfoLayout = new HorizontalStackLayout
            {
                Spacing = 8,
                VerticalOptions = LayoutOptions.Start

            };
            var colabIcon = new Image
            {
                Source = "user_icon.png",
                WidthRequest = 20,
                HeightRequest = 20,
                VerticalOptions = LayoutOptions.Center
            };
            var colabNameLabel = new Label
            {
                Text = colaboradorNome,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#003366"),
                VerticalTextAlignment = TextAlignment.Center
            };
            colabInfoLayout.Children.Add(colabIcon);
            colabInfoLayout.Children.Add(colabNameLabel);
            detailsLayout.Children.Add(colabInfoLayout);
        }

        var shiftTitleLabel = new Label
        {
            Text = fullShiftTitleForDetails,
            FontSize = 14,
            Margin = !isPrincipalView ? new Thickness(0, 5, 0, 0) : new Thickness(0, 0, 0, 0),
            TextColor = Colors.Black,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Start

        };
        detailsLayout.Children.Add(shiftTitleLabel);


        Grid.SetColumn(detailsLayout, 1);
        listItem.Children.Add(detailsLayout);

        return listItem;
    }

    private async Task DisplayPrincipalColaboradorShiftsAsync()
    {
        try
        {
            var dataStr = DataSelecionada.ToString("dd/MM/yyyy");
            var turnosPrincipalResponse = await _service.GetTurnosCalendarioAsync(this.IdColaborador, this.Token, dataStr, dataStr);
            var principalShifts = turnosPrincipalResponse?.Body?.GetTurnosCalendarioResult?.aTurnosCalendario;

            if (principalShifts != null && principalShifts.Length > 0)
            {
                foreach (var turno in principalShifts)
                {
                    var listItem = CreateShiftListItemView(this.nome_abreviado, turno.titulo, turno.dataHoraInicio, turno.dataHoraFim, true);
                    StackData.Children.Add(listItem);
                    var separatorLine = new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#00A2C7") };
                    StackData.Children.Add(separatorLine);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Falha ao carregar turnos do colaborador principal: " + ex.Message, "OK");
        }
    }

    private async Task CarregarPMTsAsync()
    {
        try
        {
            StackData.Children.Clear();

            await DisplayPrincipalColaboradorShiftsAsync();

            if (StackData.Children.Count == 0)
            {
                StackData.Children.Add(new Label
                {
                    Text = "Nenhum turno encontrado para esta data para o colaborador principal.",
                    TextColor = Colors.Black,
                    HorizontalTextAlignment = TextAlignment.Center,
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Falha ao carregar turnos iniciais: " + ex.Message, "OK");
        }
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        try
        {
            var dataStr = DataSelecionada.ToString("dd/MM/yyyy");
            var turnosPrincipalResponse = await _service.GetTurnosCalendarioAsync(this.IdColaborador, this.Token, dataStr, dataStr);
            var principalShifts = turnosPrincipalResponse?.Body?.GetTurnosCalendarioResult?.aTurnosCalendario;

            if (principalShifts != null && principalShifts.Length > 0)
            {
                string titulo = principalShifts[0].titulo?.Trim().ToUpper() ?? "";
                if (titulo == "DS" || titulo == "DC" || titulo.Contains("FER"))
                {
                    return;
                }
            }

            isOutrosColaboradoresVisible = !isOutrosColaboradoresVisible;

            if (isOutrosColaboradoresVisible)
            {
                VerOutrosIcon.Source = "circulo_activo.png";

                StackData.Children.Clear();
                await DisplayPrincipalColaboradorShiftsAsync();

                var respostaColabs = await _service.GetColaboradoresAsync(IdPMT, this.IdColaborador, Token);
                var todosOutrosColabs = respostaColabs?.Body?.GetColaboradoresResult?.aColaboradoresPMT;

                var respostaTurnos = await _service.GetTurnosCalendarioAsync(this.IdColaborador, Token, dataStr, dataStr);
                var turnosParaOutros = respostaTurnos?.Body?.GetTurnosCalendarioResult?.aTurnosCalendario;

                if (todosOutrosColabs != null && todosOutrosColabs.Length > 0 && turnosParaOutros != null && turnosParaOutros.Length > 0)
                {
                    foreach (var outroColab in todosOutrosColabs)
                    {
                        foreach (var turno in turnosParaOutros)
                        {
                            var listItem = CreateShiftListItemView(outroColab.nomeAbreviado, turno.titulo, turno.dataHoraInicio, turno.dataHoraFim, false);
                            StackData.Children.Add(listItem);
                            var separatorLine = new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#00A2C7") };
                            StackData.Children.Add(separatorLine);
                        }
                    }
                }
            }
            else
            {
                VerOutrosIcon.Source = "circulo_peq.png";
                StackData.Children.Clear();
                await DisplayPrincipalColaboradorShiftsAsync();
            }
        }
        catch (Exception ex)
        {
            isOutrosColaboradoresVisible = false;
            VerOutrosIcon.Source = "circulo_peq.png";
            await DisplayAlert("Erro", "Erro ao carregar outros colaboradores: " + ex.Message, "OK");
        }
    }
}