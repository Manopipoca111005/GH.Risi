using MauiApp1;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Xml.Linq;
using GH_Metodos;
using Microsoft.Maui.ApplicationModel; // Necessário para Browser.Default

namespace TarefasApp;

public partial class TarefasPage : ContentPage, INotifyPropertyChanged
{
    public const string OpcaoDefinicoes = "Definiçőes";
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";
    public event PropertyChangedEventHandler PropertyChanged;
    private readonly string Token;
    private readonly int Idcolaborador;

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged(nameof(IsBusy));
        }
    }

    public ObservableCollection<TarefaPendente> TarefasParaExibir { get; } = new();
    public Command LoadDataCommand { get; }


    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);

    public TarefasPage(int idcolaborador, string token)
    {
        InitializeComponent();
        BindingContext = this;

        Idcolaborador = idcolaborador;
        Token = token;


        LoadDataCommand = new Command(async () => await CarregarTarefas());
        LoadDataCommand.Execute(null);
    }

    protected virtual void OnPropertyChanged(string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async Task CarregarTarefas()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            TarefasParaExibir.Clear();

            var tarefas = await ObterTarefasDoWebService();

            if (tarefas == null || !tarefas.Any())
            {
                await DisplayAlert("Informação", "Nenhuma tarefa pendente encontrada.", "OK");
                return;
            }

            foreach (var tarefa in tarefas)
            {
                TarefasParaExibir.Add(tarefa);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<List<TarefaPendente>> ObterTarefasDoWebService()
    {
        var tarefas = new List<TarefaPendente>();

        try
        {
            var response = await _service.GetTarefasPendentesAsync(Idcolaborador, Token);
            var result = response.Body?.GetTarefasPendentesResult;

            if (result == null)
            {
                throw new Exception("Resposta inválida do servidor.");
            }

            if (result._erro != null && result._erro.erro != 0)
            {
                throw new Exception($"Erro do Webservice: {result._erro.erroMensagem}");
            }

            if (result.aTarefasPendentes == null || !result.aTarefasPendentes.Any())
            {
                return tarefas;
            }

            foreach (var item in result.aTarefasPendentes)
            {
                if (item == null) continue;

                tarefas.Add(new TarefaPendente
                {
                    Tipo = item.tipo,
                    IdPerfil = item.idPerfil,
                    DescTarefa = item.descTarefa ?? "Sem descrição",
                    Ano = item.ano,
                    Mes = item.mes,
                    DescMes = item.descMes ?? string.Empty,
                    IdServico = item.idServico,
                    DescServico = item.descServico ?? "Sem serviço",
                    IdsServicos = item.idsServicos ?? string.Empty,
                    Dados = item.dados ?? string.Empty,
                    url = Remember.site,
                    LinkTarefa = $"{Remember.site}?taskMobile=4&ticket={Token}&idPerfil=7&par1={item.ano}&par2={item.mes}"

                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao obter tarefas: {ex}");
            throw;
        }

        return tarefas;
    }

   
    private async void OnTarefaItemTapped(object sender, TappedEventArgs e)
    {
        
        if (sender is VisualElement tappedElement && tappedElement.BindingContext is TarefaPendente tappedTarefa)
        {
            if (!string.IsNullOrEmpty(tappedTarefa.LinkTarefa))
            {
                try
                {
                    await Browser.Default.OpenAsync(tappedTarefa.LinkTarefa, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", $"Não foi possível abrir o link: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Informação", "Nenhum link associado a esta tarefa.", "OK");
            }
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
}


public class TarefaPendente
{
    public short Tipo { get; set; }
    public short IdPerfil { get; set; }
    public string DescTarefa { get; set; }
    public short Ano { get; set; }
    public short Mes { get; set; }
    public string DescMes { get; set; }
    public int IdServico { get; set; }
    public string DescServico { get; set; }
    public string IdsServicos { get; set; }
    public string Dados { get; set; }
    public string LinkTarefa { get; set; }
    public string url { get; set; }
    public string CabecalhoFormatado => DescServico;
    public string PeriodoFormatado => $"{DescMes?.ToUpper()} {Ano}";
}