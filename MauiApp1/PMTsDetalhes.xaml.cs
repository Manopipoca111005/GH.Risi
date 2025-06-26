using CommunityToolkit.Maui.Views;
using GH_Metodos;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
namespace MauiApp1
{

    public class Colaborador
    {
        public int idColaborador { get; set; }
        public int idColaboradorPMT { get; set; }
        public string descColaborador { get; set; }

    }

    public partial class PMTsDetalhes : ContentPage
    {
        private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
        public const string OpcaoDefinicoes = "Definições";
        public const string OpcaoSair = "Sair da app";
        public const string OpcaoCancelar = "Cancelar";
        private readonly int IdColaborador;
        private readonly string Token;
        private readonly string mesano;
        private readonly string nome_abreviado;
        private readonly int IdPMT;
        private string site;

        public PMTsDetalhes(int id_colaborador, string token, string mesAnoFormatado, string NomeAbreviado, int idPMT)
        {
            InitializeComponent();
            IdColaborador = id_colaborador;
            Token = token;
            mesano = mesAnoFormatado;
            nome_abreviado = NomeAbreviado;
            User.Text = nome_abreviado;
            IdPMT = idPMT;
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

        private async Task CarregarPMTsAsync()
        {
            try
            {
                CultureInfo culture = new CultureInfo("pt-PT");
                DateTime parsedMesAno;
                int ano = DateTime.Now.Year;
                int mes = DateTime.Now.Month;

                if (DateTime.TryParseExact(mesano, "MMMM 'de' yyyy", culture, DateTimeStyles.None, out parsedMesAno))
                {
                    ano = parsedMesAno.Year;
                    mes = parsedMesAno.Month;
                }
                else if (DateTime.TryParseExact(mesano, "MMMM", culture, DateTimeStyles.None, out var monthOnly))
                {
                    ano = DateTime.Now.Year;
                    mes = monthOnly.Month;
                }
                else if (DateTime.TryParse(mesano, culture, DateTimeStyles.None, out var anyFormat))
                {
                    ano = anyFormat.Year;
                    mes = anyFormat.Month;
                }
                else
                {
                    await DisplayAlert("Erro", $"Formato de mês/ano inválido: '{mesano}'. Esperado 'Mês de Ano' ou 'Mês'.", "OK");
                    return;
                }

                var datainicio = new DateTime(ano, mes, 1);
                var datafim = new DateTime(ano, mes, DateTime.DaysInMonth(ano, mes));

                var dataInicioStr = datainicio.ToString("dd/MM/yyyy");
                var dataFimStr = datafim.ToString("dd/MM/yyyy");

                var resposta = await _service.GetTurnosCalendarioAsync(IdColaborador, Token, dataInicioStr, dataFimStr);
                var pmts = resposta?.Body?.GetTurnosCalendarioResult?.aTurnosCalendario;

                StackDetalhesPMTs.Children.Clear();
                if (pmts != null && pmts.Length > 0)
                {
                    foreach (var item in pmts)
                    {
                        DateTimeOffset inicio, fim;
                        try
                        {
                            inicio = DateTimeOffset.ParseExact(item.dataHoraInicio, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
                            fim = DateTimeOffset.ParseExact(item.dataHoraFim, "yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);
                        }
                        catch (FormatException)
                        {
                            continue;
                        }
                        DateTime itemDate = inicio.Date;

                        var itemLayoutGrid = new Grid
                        {
                            ColumnSpacing = 0,
                            RowSpacing = 0,
                            BackgroundColor = Colors.White,
                            Padding = new Thickness(0),
                            ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition { Width = new GridLength(8) },
                            new ColumnDefinition { Width = new GridLength(65) },
                            new ColumnDefinition { Width = GridLength.Star },
                            new ColumnDefinition { Width = new GridLength(30) }
                        }
                        };

                        string corStringParaParsear = "#1399c0";
                        Color sideBarBackgroundColor;

                        string tituloOriginal = item.titulo;
                        string tituloUpper = "";

                        if (!string.IsNullOrWhiteSpace(tituloOriginal))
                        {
                            tituloUpper = tituloOriginal.ToUpperInvariant();

                            if (tituloUpper.Contains("FF") || tituloUpper.Contains("FE"))
                            {
                                if (!string.IsNullOrEmpty(item.cor))
                                {
                                    corStringParaParsear = item.cor;
                                }
                            }
                        }

                        try
                        {
                            sideBarBackgroundColor = Color.FromArgb(corStringParaParsear);
                        }
                        catch
                        {
                            sideBarBackgroundColor = Color.FromArgb("#1399c0");
                        }

                        var sideBar = new BoxView
                        {
                            BackgroundColor = sideBarBackgroundColor,
                            VerticalOptions = LayoutOptions.FillAndExpand
                        };
                        Grid.SetColumn(sideBar, 0);
                        itemLayoutGrid.Children.Add(sideBar);

                        Color dateBoxBackgroundColor = Colors.Transparent;
                        if (!string.IsNullOrWhiteSpace(tituloUpper) && tituloUpper.Contains("FER"))
                        {
                            dateBoxBackgroundColor = Color.FromArgb("#808080");
                        }
                        else if (itemDate.DayOfWeek == DayOfWeek.Sunday)
                        {
                            dateBoxBackgroundColor = Color.FromArgb("#B3B3B3");
                        }
                        else if (itemDate.DayOfWeek == DayOfWeek.Saturday)
                        {
                            dateBoxBackgroundColor = Color.FromArgb("#E6E6E6");
                        }

                        var dateStack = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        dateStack.Children.Add(new Label { Text = itemDate.ToString("MMMM", culture).ToUpper(), TextColor = Colors.Black, FontSize = 10, HorizontalTextAlignment = TextAlignment.Center });
                        dateStack.Children.Add(new Label { Text = inicio.Day.ToString("D2"), TextColor = Colors.Black, FontAttributes = FontAttributes.Bold, FontSize = 20, HorizontalTextAlignment = TextAlignment.Center });
                        dateStack.Children.Add(new Label { Text = itemDate.ToString("dddd", culture).ToLower(), TextColor = Colors.Black, FontSize = 9, HorizontalTextAlignment = TextAlignment.Center });

                        var dateTextContainer = new Frame
                        {
                            BackgroundColor = dateBoxBackgroundColor,
                            Padding = new Thickness(5, 8, 5, 8),
                            HasShadow = false,
                            CornerRadius = 0,
                            BorderColor = Colors.Transparent,
                            VerticalOptions = LayoutOptions.FillAndExpand,
                            Content = dateStack
                        };
                        Grid.SetColumn(dateTextContainer, 1);
                        itemLayoutGrid.Children.Add(dateTextContainer);

                        var detailsStack = new VerticalStackLayout { Padding = new Thickness(10, 5, 5, 5), VerticalOptions = LayoutOptions.Center, Spacing = 2 };
                        var titleLabel = new Label { FontSize = 13, VerticalTextAlignment = TextAlignment.Center };
                        if (item.titulo != null && (item.titulo.StartsWith("F2") || item.titulo.StartsWith("T") || item.titulo.StartsWith("F1")))
                        {
                            var formattedTitle = new FormattedString();
                            int parenthesisIndex = item.titulo.IndexOf('(');
                            if (parenthesisIndex > 0)
                            {
                                formattedTitle.Spans.Add(new Span { Text = item.titulo.Substring(0, parenthesisIndex).Trim() + " ", TextColor = Colors.Black });
                                formattedTitle.Spans.Add(new Span { Text = item.titulo.Substring(parenthesisIndex), TextColor = Color.FromArgb("#007BA7") });
                            }
                            else
                            {
                                formattedTitle.Spans.Add(new Span { Text = item.titulo, TextColor = Colors.Black });
                            }
                            titleLabel.FormattedText = formattedTitle;
                        }
                        else
                        {
                            titleLabel.Text = item.titulo;
                            titleLabel.TextColor = Colors.Black;
                        }
                        detailsStack.Children.Add(titleLabel);
                        Grid.SetColumn(detailsStack, 2);
                        itemLayoutGrid.Children.Add(detailsStack);


                        StackDetalhesPMTs.Children.Add(itemLayoutGrid);

                        var separatorLine = new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#E0E0E0"), Margin = new Thickness(0) };
                        StackDetalhesPMTs.Children.Add(separatorLine);
                        var tapGesture = new TapGestureRecognizer();
                        Preferences.Set("pmt", IdPMT);
                        tapGesture.Tapped += async (s, e) =>
                        {
                            await global::Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.PushAsync(
                                new PMTsDetalhesTurnos(IdColaborador, Token, itemDate, nome_abreviado, IdPMT));
                           
                        };
                        itemLayoutGrid.GestureRecognizers.Add(tapGesture);
                    }

                }
                else
                {
                    StackDetalhesPMTs.Children.Add(new Label { Text = "Nenhuma notificação encontrada.", Padding = new Thickness(10), HorizontalTextAlignment = TextAlignment.Center });
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Falha ao carregar notificações: " + ex.Message, "OK");
            }
        }

        private async void TapGestureRecognizer_Tapped_2(object sender, TappedEventArgs e)
        {
            try
            {
                var binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

                var endpoint = new EndpointAddress("https://sites.risi.pt/wsGHMobile4Testes/Service6.asmx");
                var clientForThisCall = new Service6SoapClient(binding, endpoint);

                var result = await clientForThisCall.GetColaboradoresAsync(IdPMT, IdColaborador, Token);
                var colaboradoresFromService = result?.Body?.GetColaboradoresResult?.aColaboradoresPMT;



                if (colaboradoresFromService == null || colaboradoresFromService.Length == 0)
                {
                    await DisplayAlert("Erro", "Não foi possível obter colaboradores ou a lista está vazia.", "OK");
                    return;
                }

                var colaboradoresParaPopup = colaboradoresFromService
                    .Where(c => c.idColaborador != IdColaborador)
                    .Select(serviceColaborador => new Colaborador
                    {
                        idColaboradorPMT = serviceColaborador.idColaborador,
                        descColaborador = serviceColaborador.nomeAbreviado
                    })
                    .ToList();

                if (!colaboradoresParaPopup.Any())
                {
                    await DisplayAlert("Informação", "Não há outros colaboradores para selecionar.", "OK");
                    return;
                }

                var popup = new SelecionaColaboradoresPopup(colaboradoresParaPopup);
                var colaboradorSelecionado = await this.ShowPopupAsync(popup) as Colaborador;

                if (colaboradorSelecionado == null)
                    return;

                User.Text = colaboradorSelecionado.descColaborador;
                StackDetalhesPMTs.Children.Clear();

                if (colaboradorSelecionado.idColaboradorPMT == IdColaborador)
                {
                    await CarregarPMTsAsync();
                    return;
                }
                

                var diasResult = await clientForThisCall.GetDiasAsync(IdPMT, colaboradorSelecionado.idColaboradorPMT, IdColaborador, Token);
                var dias = diasResult?.Body?.GetDiasResult?.aDias;

                if (dias == null || dias.Length == 0)
                {
                    StackDetalhesPMTs.Children.Add(new Label
                    {
                        Text = "Nenhuma informação de dias encontrada para o colaborador selecionado.",
                        Padding = new Thickness(10),
                        HorizontalTextAlignment = TextAlignment.Center
                    });
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Obtidos {dias.Length} dias para o colaborador {colaboradorSelecionado.descColaborador}");
                CultureInfo culture = new CultureInfo("pt-PT");

                var parsedDias = dias
                        .Select(d => new {
                            OriginalDataString = d.data,
                            OriginalDiaObject = d,
                            IsParsable = DateTime.TryParse(d.data, culture, DateTimeStyles.None, out var parsedDt),
                            ParsedDateValue = parsedDt
                        })
                        .Where(x => x.IsParsable)
                        .ToList();

                var diasUnicos = parsedDias
                         .GroupBy(x => x.OriginalDataString)
                         .Select(g => g.First())
                         .OrderBy(x => x.ParsedDateValue.Date)
                         .Select(x => x.OriginalDiaObject)
                         .ToList();

                if (!diasUnicos.Any())
                {
                    StackDetalhesPMTs.Children.Add(new Label
                    {
                        Text = "Nenhuma informação válida de dias encontrada para o colaborador selecionado.",
                        Padding = new Thickness(10),
                        HorizontalTextAlignment = TextAlignment.Center
                    });
                    return;
                }

                foreach (var dia in diasUnicos)
                {
                    if (!int.TryParse(dia.idPMTColabDia, out int idPMTColabDia))
                    {
                        continue;
                    }

                    var turnosResult = await clientForThisCall.GetTurnosAsync(idPMTColabDia, IdColaborador, Token);
                    var turnos = turnosResult?.Body?.GetTurnosResult?.aTurnos;

                    if (turnos == null || turnos.Length == 0)
                    {
                        continue;
                    }

                    var turnosFiltrados = turnos.Where(t => t.idColaborador == colaboradorSelecionado.idColaboradorPMT).ToArray();
                    if (!turnosFiltrados.Any())
                    {
                        continue;
                    }

                    DateTime parsedDiaData = DateTime.TryParse(dia.data, culture, DateTimeStyles.None, out var dt) ? dt : DateTime.Today;

                    foreach (var turno in turnosFiltrados)
                    {
                        var itemLayoutGrid = new Grid
                        {
                            ColumnSpacing = 0,
                            RowSpacing = 0,
                            BackgroundColor = Colors.White,
                            Padding = new Thickness(0),
                            ColumnDefinitions = new ColumnDefinitionCollection
                            {
                                new ColumnDefinition { Width = new GridLength(8) },
                                new ColumnDefinition { Width = new GridLength(65) },
                                new ColumnDefinition { Width = GridLength.Star },
                                new ColumnDefinition { Width = new GridLength(30) }
                            }
                        };

                        string corStringParaParsearSidebar = "#1399c0";
                        string textoParaCor = turno.sigla ?? "";
                        string textoParaCorUpper = "";
                        Color sideBarBackgroundColor;


                        if (!string.IsNullOrWhiteSpace(textoParaCor))
                        {
                            textoParaCorUpper = textoParaCor.ToUpperInvariant();


                            if (textoParaCorUpper.Contains("FF") || textoParaCorUpper.Contains("FE"))
                            {
                                if (!string.IsNullOrEmpty(dia.cor))
                                {
                                    corStringParaParsearSidebar = dia.cor;

                                }
                            }
                        }

                        try
                        {
                            sideBarBackgroundColor = Color.FromArgb(corStringParaParsearSidebar);
                        }
                        catch
                        {
                            sideBarBackgroundColor = Color.FromArgb("#1399c0");
                        }

                        var sideBar = new BoxView { BackgroundColor = sideBarBackgroundColor, VerticalOptions = LayoutOptions.FillAndExpand };
                        Grid.SetColumn(sideBar, 0);
                        itemLayoutGrid.Children.Add(sideBar);

                        Color dateBoxBackgroundColor = Colors.Transparent;
                        if (!string.IsNullOrWhiteSpace(textoParaCorUpper) && textoParaCorUpper.Contains("FER"))
                        {
                            dateBoxBackgroundColor = Color.FromArgb("#808080");
                        }
                        else if (parsedDiaData.DayOfWeek == DayOfWeek.Sunday)
                        {
                            dateBoxBackgroundColor = Color.FromArgb("#B3B3B3");
                        }
                        else if (parsedDiaData.DayOfWeek == DayOfWeek.Saturday)
                        {
                            dateBoxBackgroundColor = Color.FromArgb("#E6E6E6");
                        }

                        var dateStack = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
                        dateStack.Children.Add(new Label { Text = parsedDiaData.ToString("MMMM", culture).ToUpper(), TextColor = Colors.Black, FontSize = 10, HorizontalTextAlignment = TextAlignment.Center });
                        dateStack.Children.Add(new Label { Text = parsedDiaData.Day.ToString("D2"), TextColor = Colors.Black, FontAttributes = FontAttributes.Bold, FontSize = 20, HorizontalTextAlignment = TextAlignment.Center });
                        dateStack.Children.Add(new Label { Text = parsedDiaData.ToString("dddd", culture).ToLower(), TextColor = Colors.Black, FontSize = 9, HorizontalTextAlignment = TextAlignment.Center });

                        var dateTextContainer = new Frame
                        {
                            BackgroundColor = dateBoxBackgroundColor,
                            Padding = new Thickness(5, 8, 5, 8),
                            HasShadow = false,
                            CornerRadius = 0,
                            BorderColor = Colors.Transparent,
                            VerticalOptions = LayoutOptions.FillAndExpand,
                            Content = dateStack
                        };
                        Grid.SetColumn(dateTextContainer, 1);
                        itemLayoutGrid.Children.Add(dateTextContainer);

                        var detailsStack = new VerticalStackLayout { Padding = new Thickness(10, 5, 5, 5), VerticalOptions = LayoutOptions.Center, Spacing = 2 };
                        var titleLabel = new Label { FontSize = 13, VerticalTextAlignment = TextAlignment.Center };

                        string titleContent = turno.sigla;
                        string prefix = turno.nomeAbreviado;

                        if (!string.IsNullOrEmpty(prefix) && (prefix.StartsWith("F2") || prefix.StartsWith("T") || prefix.StartsWith("F1")))
                        {
                            var formattedTitle = new FormattedString();
                            int parenthesisIndex = titleContent?.IndexOf('(') ?? -1;

                            if (!string.IsNullOrEmpty(titleContent) && parenthesisIndex > 0 && titleContent.EndsWith(")"))
                            {
                                formattedTitle.Spans.Add(new Span { Text = titleContent.Substring(0, parenthesisIndex).Trim() + " ", TextColor = Colors.Black });
                                formattedTitle.Spans.Add(new Span { Text = titleContent.Substring(parenthesisIndex), TextColor = Color.FromArgb("#007BA7") });
                            }
                            else
                            {
                                formattedTitle.Spans.Add(new Span { Text = $" {titleContent}".Trim(), TextColor = Colors.Black });
                            }
                            titleLabel.FormattedText = formattedTitle;

                        }
                        else
                        {
                            titleLabel.Text = $" {titleContent} {turno.horaInicio} {turno.horaFim}".Trim();
                            titleLabel.TextColor = Colors.Black;
                        }
                        detailsStack.Children.Add(titleLabel);

                        Grid.SetColumn(detailsStack, 2);
                        itemLayoutGrid.Children.Add(detailsStack);

                        StackDetalhesPMTs.Children.Add(itemLayoutGrid);
                        var separatorLine = new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#E0E0E0"), Margin = new Thickness(0) };
                        StackDetalhesPMTs.Children.Add(separatorLine);
                        var tapGesture = new TapGestureRecognizer();
                        tapGesture.Tapped += async (s, e) =>
                        {
                            await global::Microsoft.Maui.Controls.Application.Current.MainPage.Navigation.PushAsync(
                                new SelecionaTurnosColaboradores(IdColaborador, Token, parsedDiaData, colaboradorSelecionado.descColaborador, IdPMT, idPMTColabDia));
                        };
                        itemLayoutGrid.GestureRecognizers.Add(tapGesture);
                       

                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Ocorreu um erro ao carregar os detalhes do colaborador: " + ex.Message, "OK");
            }
        }

        private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
        {
            try
            {
                string url = Remember.site;
                if (string.IsNullOrEmpty(url))
                {
                    await DisplayAlert("Erro", "O URL de base não foi configurado.", "OK");
                    return;
                }

                site = $"{url}?taskMobile=0&ticket={Token}&par1={IdPMT}";

                if (Uri.TryCreate(site, UriKind.Absolute, out Uri uriResult))
                {
                    await Navigation.PushAsync(new WebViewPage(uriResult.ToString()));
                }
                else
                {
                    await DisplayAlert("Erro", "URL inválida.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Não foi possível abrir o link: " + ex.Message, "OK");
            }
        }

    }
}