using GH_Metodos;
using Google.Apis.Calendar.v3;
using System.Text;
using System.Threading.Tasks;
using MauiApp1.Services;
using TarefasApp;
using System.Diagnostics;

namespace MauiApp1;

public partial class OptionsPage : ContentPage
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    private readonly int id_colaborador;
    private readonly string token;
    private readonly string nome_abreviado;
    private readonly bool Novas;
    private readonly CalendarioHelper _calendarioHelper;
    private readonly ICalendarService _calendarService;

    public const string OpcaoDefinicoes = "Definições";
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";

    public OptionsPage(int IdColaborador, string Token, string NomeAbreviado, ICalendarService calendarService)
    {
        InitializeComponent();
        id_colaborador = IdColaborador;
        token = Token;
        nome_abreviado = NomeAbreviado;
        NomeLabel.Text = nome_abreviado;
        IdLabel.Text = id_colaborador.ToString();

        _calendarioHelper = new CalendarioHelper(_service, id_colaborador, token);

        _calendarService = calendarService;

        _ = ContarNotificacoesAsync();
        _ = ContarDisponibilidadesAsync();
        _ = ContarTrocasAsync();

        FeriasCheckBox.IsChecked = true;
        AbstinenciasCheckBox.IsChecked = true;
        TurnosCheckBox.IsChecked = true;
    }

    private async Task ContarNotificacoesAsync()
    {
        try
        {
            var resposta = await _service.GetContabilizacoesAsync(id_colaborador, token);
            var notificacoes = resposta?.Body?.GetContabilizacoesResult;

            if (notificacoes != null)
            {
                int totalNotificacoes = notificacoes.nrNotificacoes;
                NotificationCountLabel.Text = totalNotificacoes.ToString();
                NotificationBadgeBorder.IsVisible = totalNotificacoes > 0;
            }
            else
            {
                NotificationBadgeBorder.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro ao carregar as notificações: {ex.Message}", "OK");
            NotificationBadgeBorder.IsVisible = false;
        }
    }

    private async Task ContarDisponibilidadesAsync()
    {
        try
        {
            var resposta = await _service.GetContabilizacoesAsync(id_colaborador, token);
            var disponibilidades = resposta?.Body?.GetContabilizacoesResult;

            if (disponibilidades != null)
            {
                int totalDisponibilidades = disponibilidades.nrDisponibilidadesAbertas;
                DisponibilidadeCountLabel.Text = totalDisponibilidades.ToString();
                DisponibilidadeBadgeBorder.IsVisible = totalDisponibilidades > 0;
            }
            else
            {
                DisponibilidadeBadgeBorder.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro ao carregar as disponibilidades: {ex.Message}", "OK");
            DisponibilidadeBadgeBorder.IsVisible = false;
        }
    }

    private async Task ContarTrocasAsync()
    {
        try
        {
            var resposta = await _service.GetContabilizacoesAsync(id_colaborador, token);
            var trocas = resposta?.Body?.GetContabilizacoesResult;

            if (trocas != null)
            {
                int totalTrocas = trocas.nrTrocasPorValidar;
                TrocasCountLabel.Text = totalTrocas.ToString();
                TrocasBadgeBorder.IsVisible = totalTrocas > 0;
            }
            else
            {
                TrocasBadgeBorder.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro ao carregar as trocas: {ex.Message}", "OK");
            TrocasBadgeBorder.IsVisible = false;
        }
    }

    private async void ShowPopup()
    {
        PopupOverlay.IsVisible = true;

        PopupFrame.Scale = 0.8;
        PopupFrame.Opacity = 0;

        await Task.WhenAll(
            PopupFrame.ScaleTo(1, 250, Easing.CubicOut),
            PopupFrame.FadeTo(1, 250)
        );
        StatusSincronizacaoLabel.IsVisible = true;
    }

    private async void HidePopup()
    {
        await Task.WhenAll(
            PopupFrame.ScaleTo(0.8, 200, Easing.CubicIn),
            PopupFrame.FadeTo(0, 200)
        );

        PopupOverlay.IsVisible = false;

        StatusSincronizacaoLabel.IsVisible = false;
        StatusSincronizacaoLabel.Text = "";
    }

    private async void Button_Clicked_9(object sender, EventArgs e)
    {
        ShowPopup();
    }

    private async void ClosePopup_Clicked(object sender, EventArgs e)
    {
        HidePopup();
    }

    private async void OkButton_Clicked(object sender, EventArgs e)
    {
        HidePopup();
    }

    private async void PopupOverlay_Tapped(object sender, TappedEventArgs e)
    {
        HidePopup();
    }

    private async void SincronizarButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            StatusSincronizacaoLabel.IsVisible = true;
            StatusSincronizacaoLabel.Text = "A sincronizar...";
            StatusSincronizacaoLabel.TextColor = Colors.Orange;

            var tarefasSincronizacao = new List<Task<string>>();
            var resultados = new StringBuilder();

            if (FeriasCheckBox.IsChecked)
            {
                tarefasSincronizacao.Add(SincronizarFeriasAsync());
            }

            if (AbstinenciasCheckBox.IsChecked)
            {
                tarefasSincronizacao.Add(SincronizarAbstinenciasAsync());
            }

            if (TurnosCheckBox.IsChecked)
            {
                tarefasSincronizacao.Add(SincronizarTurnosAsync());
            }

            if (tarefasSincronizacao.Count == 0)
            {
                StatusSincronizacaoLabel.Text = "Nenhuma opção selecionada";
                StatusSincronizacaoLabel.TextColor = Colors.Red;
                return;
            }

            var resultadosTarefas = await Task.WhenAll(tarefasSincronizacao);

            foreach (var resultado in resultadosTarefas)
            {
                resultados.AppendLine(resultado);
            }

            StatusSincronizacaoLabel.Text = "Sincronização concluída!";
            StatusSincronizacaoLabel.TextColor = Colors.Green;

            await DisplayAlert("Sincronização Concluída", resultados.ToString(), "OK");

            await Task.Delay(1000);
            HidePopup();
        }
        catch (Exception ex)
        {
            StatusSincronizacaoLabel.Text = "Erro na sincronização";
            StatusSincronizacaoLabel.TextColor = Colors.Red;
            await DisplayAlert("Erro", $"Ocorreu um erro durante a sincronização: {ex.Message}", "OK");
        }
    }

    private async Task<string> SincronizarFeriasAsync()
    {
        try
        {
            DateTime dataInicio = DateTime.Now.AddMonths(-6).Date;
            DateTime dataFim = DateTime.Now.AddMonths(6).Date;

            var ferias = await _calendarioHelper.ObterFeriasAsync(dataInicio, dataFim);

            if (ferias != null && ferias.Any())
            {
                foreach (var feria in ferias)
                {
                    _calendarService.AddEventToCalendar(
                        title: feria.Titulo,
                        description: feria.ResumoEvento,
                        location: "",
                        startTime: feria.DataHoraInicio.GetValueOrDefault(),
                        endTime: feria.DataHoraFim.GetValueOrDefault(),
                        allDay: true
                    );
                }
                return $"Férias: {ferias.Count} eventos enviados para o calendário.";
            }
            return "Férias: Nenhum evento para enviar ao calendário.";
        }
        catch (Exception ex)
        {
            return $"Férias: Erro - {ex.Message}";
        }
    }

    private async Task<string> SincronizarAbstinenciasAsync()
    {
        try
        {
            DateTime dataInicio = DateTime.Now.AddMonths(-6).Date;
            DateTime dataFim = DateTime.Now.AddMonths(6).Date;

            var abstinencias = await _calendarioHelper.ObterAbstinenciasAsync(dataInicio, dataFim);

            if (abstinencias != null && abstinencias.Any())
            {
                foreach (var abstinencia in abstinencias)
                {
                    _calendarService.AddEventToCalendar(
                        title: abstinencia.Titulo,
                        description: abstinencia.ResumoEvento,
                        location: "",
                        startTime: abstinencia.DataHoraInicio.GetValueOrDefault(),
                        endTime: abstinencia.DataHoraFim.GetValueOrDefault(),
                        allDay: true
                    );
                }
                return $"Abstinências: {abstinencias.Count} eventos enviados para o calendário.";
            }
            return "Abstinências: Nenhum evento para enviar ao calendário.";
        }
        catch (Exception ex)
        {
            return $"Abstinências: Erro - {ex.Message}";
        }
    }

    private async Task<string> SincronizarTurnosAsync()
    {
        try
        {
            DateTime dataInicio = DateTime.Now.Date;
            DateTime dataFim = DateTime.Now.AddMonths(2).Date;

            var turnos = await _calendarioHelper.ObterTurnosTrabalhoAsync(dataInicio, dataFim);

            if (turnos != null && turnos.Any())
            {
                foreach (var turno in turnos)
                {
                    Trace.WriteLine($"Enviando turno: {turno.ToString()} ({turno.DataHoraInicio} - {turno.DataHoraFim})");
                    _calendarService.AddEventToCalendar(
                        title: turno.Titulo,
                        description: turno.ResumoEvento,
                        location: "",
                        startTime: turno.DataHoraInicio.GetValueOrDefault(),
                        endTime: turno.DataHoraFim.GetValueOrDefault(),
                        allDay: false
                    );
                }
                return $"Turnos: {turnos.Count} eventos enviados para o calendário.";
            }
            return "Turnos: Nenhum evento para enviar ao calendário.";
        }
        catch (Exception ex)
        {
            return $"Turnos: Erro - {ex.Message}";
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
                    await DisplayAlert("Ação", "Operação cancelada.", "OK");
                    break;
            }
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new DadosPessoais(id_colaborador, token, nome_abreviado));
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Notificacoes(id_colaborador, token, nome_abreviado));
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AssiduidadePage(id_colaborador, token, nome_abreviado));
    }

    private async void Button_Clicked_3(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PMTs(id_colaborador, token, nome_abreviado));
    }

    private async void Button_Clicked_4(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegistoFaltas(id_colaborador, token, nome_abreviado));
    }

    private async void Button_Clicked_5(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Ferias(id_colaborador, token));
    }

    private async void Button_Clicked_6(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Trocas(id_colaborador, token, nome_abreviado));
    }

    private async void Button_Clicked_7(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Disponibilidades(id_colaborador, token));
    }

    private async void Button_Clicked_8(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegistoPonto(id_colaborador, token));
    }

    private async void Button_Clicked_10(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TarefasPage(id_colaborador, token));
    }
}