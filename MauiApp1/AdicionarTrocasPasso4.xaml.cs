using Microsoft.Maui.Controls;
using System.Globalization;
using GH_Metodos;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace MauiApp1;

public partial class AdicionarTrocasPasso4 : ContentPage
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    public readonly string Token;
    public readonly int IdColaborador;
    public readonly int IdPMT;
    public int IdColabTroca;
    public string nomeabreviado;
    public string nomecolabtroca;
    public string DataColab;
    public string TurnosColab;
    public string TurnoTroca;
    public string Servico;
    public string DataColabTroca;
    public int idturno1;
    public int idturno2;

    public AdicionarTrocasPasso4(int idcolaborador, string token, int idpmt, int idcolab, string nome_abreviado, string nomecolab2, string datacolab1, string datacolab2, string turnocolabatual, string turnocolabTroca, string servico,int turno1,int turno2)
    {
        InitializeComponent();
        IdColaborador = idcolaborador;
        nomeabreviado = nome_abreviado;
        Token = token;
        IdColabTroca = idcolab;
        nomecolabtroca = nomecolab2;
        lblColab.Text = nomeabreviado;
        lblColab2.Text = nomecolabtroca;
        DataColab = datacolab1;
        DataColabTroca = datacolab2;
        DataColab1.Text = DataColab;
        DataColab2.Text = DataColabTroca;
        IdPMT = idpmt;
        TurnosColab = turnocolabatual;
        TurnoTroca = turnocolabTroca;
        Servico = servico;
        lblServico.Text = Servico;
        idturno1 = turno1;
        idturno2 = turno2;
      
        List<string> itensDoPicker = new List<string> { TurnosColab };
        pickerturno.ItemsSource = itensDoPicker;

        if (itensDoPicker.Contains(TurnosColab))
        {
            pickerturno.SelectedItem = TurnosColab;
        }
        else
        {
            pickerturno.SelectedIndex = 0;
        }

        List<string> itemDoPicker = new List<string> { TurnoTroca };
        pickerturno2.ItemsSource = itemDoPicker;

        if (itemDoPicker.Contains(TurnoTroca))
        {
            pickerturno2.SelectedItem = TurnoTroca;
        }
        else
        {
            pickerturno2.SelectedIndex = 0;
        }
    }

    private async void OnConfirmarTrocaClicked(object sender, EventArgs e)
    {
        try
        {
            DateTime data1Parsed;
            DateTime data2Parsed;

            if (!DateTime.TryParseExact(DataColab.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out data1Parsed))
            {
                await DisplayAlert("Erro", $"Formato de data do colaborador inválido. Valor: '{DataColab}'", "OK");
                return;
            }

            if (!DateTime.TryParseExact(DataColabTroca.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out data2Parsed))
            {
                await DisplayAlert("Erro", $"Formato de data do colaborador de troca inválido. Valor: '{DataColabTroca}'", "OK");
                return;
            }

            short dia1 = (short)data1Parsed.Day;
            short dia2 = (short)data2Parsed.Day;






            var result = await _service.SetTrocaDirectaAsync(
                IdColaborador,
                Token,
                IdPMT,
                IdColabTroca,
                dia1,
                dia2,
                idturno1,
                idturno2
               );

            if (result.Body.SetTrocaDirectaResult.erro == 0)
            {
                await DisplayAlert("Sucesso", "Troca solicitada com sucesso!", "OK");
                await Shell.Current.Navigation.PopToRootAsync();
            }
            else
            {
                await DisplayAlert("Erro", $"Erro ao solicitar troca: {result.Body.SetTrocaDirectaResult.erroMensagem}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro: {ex.Message}", "OK");
        }
    }
}