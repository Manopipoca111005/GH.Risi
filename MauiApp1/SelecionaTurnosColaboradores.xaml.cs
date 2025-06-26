using CommunityToolkit.Maui.Views;
using GH_Metodos;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel;

namespace MauiApp1
{
    public partial class SelecionaTurnosColaboradores : ContentPage
    {
        private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
        public const string OpcaoDefinicoes = "Definições";
        public const string OpcaoSair = "Sair da app";
        public const string OpcaoCancelar = "Cancelar";
        private int IdColaborador;
        private readonly int IdColaboradorOriginal;
        private readonly string Token;
        private string nome_abreviado;
        private readonly DateTime DataSelecionada;
        private readonly int IdPMT;
        private bool isOutrosColaboradoresVisible = false;
        private int idColaboradorDia;

        public SelecionaTurnosColaboradores(int id_colaborador, string token, DateTime dataselecionada, string NomeAbreviado, int idpmt, int idcolaboradordia)
        {
            InitializeComponent();
            IdColaborador = id_colaborador;
            IdColaboradorOriginal = id_colaborador;
            Token = token;
            nome_abreviado = NomeAbreviado;
            colab.Text = nome_abreviado;
            DataSelecionada = dataselecionada;
            IdPMT = idpmt;
            idColaboradorDia = idcolaboradordia;

            var cultura = new CultureInfo("pt-PT");
            MonthLabel.Text = DataSelecionada.ToString("MMM", cultura).ToUpper();
            DayLabel.Text = DataSelecionada.ToString("dd");
            DayOfWeekLabel.Text = DataSelecionada.ToString("dddd", cultura);

            var colabTapGesture = new TapGestureRecognizer();
            colabTapGesture.Tapped += TapGestureRecognizer_Tapped_2;
            colab.GestureRecognizers.Add(colabTapGesture);

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
            string textOnImage = !string.IsNullOrEmpty(turnoTituloOriginal) ? turnoTituloOriginal.Split(' ')[0] : "";

            var listItem = new Grid { };
            listItem.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            listItem.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            var imagemGridNovo = new Grid { VerticalOptions = LayoutOptions.Center };
            var imagemFile = new Image { Source = "turnos.png", };
            var labelDentroSkewed = new Label
            {
                Text = textOnImage,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            new BoxView { HeightRequest = 2, BackgroundColor = Color.FromArgb("#00A2C7"), };
            imagemGridNovo.Children.Add(imagemFile);
            imagemGridNovo.Children.Add(labelDentroSkewed);
            Grid.SetColumn(imagemGridNovo, 0);
            listItem.Children.Add(imagemGridNovo);

            var detailsLayout = new VerticalStackLayout { VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.Center };

            if (!isPrincipalView)
            {
                var colabInfoLayout = new HorizontalStackLayout { Spacing = 8, VerticalOptions = LayoutOptions.Start };
                var colabIcon = new Image { Source = "user_icon.png", WidthRequest = 20, HeightRequest = 20, VerticalOptions = LayoutOptions.Center };
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

            var shiftTitleLabel = new Label();
            string fullShiftTitle = GetFullShiftTitle(turnoTituloOriginal);
            if (!string.IsNullOrEmpty(turnoDataHoraInicio))
            {
                fullShiftTitle = $"{fullShiftTitle} ({turnoDataHoraInicio} {turnoDataHoraFim})";
            }
            shiftTitleLabel.Text = fullShiftTitle;
            shiftTitleLabel.FontSize = 14;
            shiftTitleLabel.Margin = !isPrincipalView ? new Thickness(0, 5, 0, 0) : new Thickness(0, 0, 0, 0);
            shiftTitleLabel.TextColor = Colors.Black;
            shiftTitleLabel.HorizontalTextAlignment = TextAlignment.Center;
            shiftTitleLabel.VerticalOptions = LayoutOptions.Start;

            detailsLayout.Children.Add(shiftTitleLabel);
            Grid.SetColumn(detailsLayout, 1);
            listItem.Children.Add(detailsLayout);
            return listItem;
        }

        private async Task RebuildFullListAsync()
        {
            try
            {
                StackData.Children.Clear();
                var respdata = await _service.GetTurnosAsync(idColaboradorDia, IdColaboradorOriginal, Token);
                var todosOsTurnosDoDia = respdata?.Body?.GetTurnosResult?.aTurnos;

                if (todosOsTurnosDoDia == null || !todosOsTurnosDoDia.Any())
                {
                    StackData.Children.Add(new Label { Text = "Nenhum turno encontrado para esta data.", HorizontalTextAlignment = TextAlignment.Center });
                    return;
                }

                var turnosPrincipais = new List<Turno>();
                var outrosTurnos = new List<Turno>();

                foreach (var turno in todosOsTurnosDoDia)
                {
                    // ALTERAÇÃO AQUI: Comparar pelo nome em vez do ID
                    if (turno.nomeAbreviado == this.nome_abreviado)
                    {
                        turnosPrincipais.Add(turno);
                    }
                    else
                    {
                        outrosTurnos.Add(turno);
                    }
                }

                foreach (var turno in turnosPrincipais)
                {
                    var listItem = CreateShiftListItemView(this.nome_abreviado, turno.sigla, turno.horaInicio, turno.horaFim, true);
                    StackData.Children.Add(listItem);
                }

                if (turnosPrincipais.Any())
                {
                    var separatorLine = new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#00A2C7") };
                    StackData.Children.Add(separatorLine);
                }

                string tituloPrincipal = turnosPrincipais.FirstOrDefault()?.sigla?.Trim().ToUpper() ?? "";
                bool isDiaDeFolga = tituloPrincipal == "DS" || tituloPrincipal == "DC" || tituloPrincipal.Contains("FER");

                if (isOutrosColaboradoresVisible && !isDiaDeFolga)
                {
                    foreach (var turno in outrosTurnos)
                    {
                        var listItem = CreateShiftListItemView(turno.nomeAbreviado, turno.sigla, turno.horaInicio, turno.horaFim, false);
                        StackData.Children.Add(listItem);
                        var separatorLine = new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#00A2C7") };
                        StackData.Children.Add(separatorLine);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Falha ao recarregar a lista de turnos: " + ex.Message, "OK");
            }
        }

        private async void CarregarPMTsAsync()
        {
            isOutrosColaboradoresVisible = false;
            VerOutrosIcon.Source = "circulo_peq.png";
            await RebuildFullListAsync();
        }

        private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
        {
            isOutrosColaboradoresVisible = !isOutrosColaboradoresVisible;
            VerOutrosIcon.Source = isOutrosColaboradoresVisible ? "circulo_activo.png" : "circulo_peq.png";
            await RebuildFullListAsync();
        }

        private async void TapGestureRecognizer_Tapped_2(object sender, TappedEventArgs e)
        {
            try
            {
                var binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport);
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

                var endpoint = new EndpointAddress("https://sites.risi.pt/wsGHMobile4Testes/Service6.asmx");
                var clientForThisCall = new Service6SoapClient(binding, endpoint);

                var result = await clientForThisCall.GetColaboradoresAsync(IdPMT, IdColaboradorOriginal, Token);
                var colaboradoresFromService = result?.Body?.GetColaboradoresResult?.aColaboradoresPMT;

                if (colaboradoresFromService == null || colaboradoresFromService.Length == 0)
                {
                    await DisplayAlert("Erro", "Não foi possível obter colaboradores ou a lista está vazia.", "OK");
                    return;
                }

                var colaboradoresParaPopup = colaboradoresFromService
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

                this.IdColaborador = colaboradorSelecionado.idColaboradorPMT;
                this.nome_abreviado = colaboradorSelecionado.descColaborador;
                colab.Text = this.nome_abreviado;

                CarregarPMTsAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", "Ocorreu um erro ao carregar os detalhes do colaborador: " + ex.Message, "OK");
            }
        }
    }
}