using GH_Metodos;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MauiApp1;

public class AnexoWithDisplayInfo : INotifyPropertyChanged
{
    public Anexo AnexoData { get; }
    public string NomeFicheiro => AnexoData.nomeFicheiro;
    public ImageSource ThumbnailSource { get; }
    public ICommand RemoveCommand { get; set; }

    public AnexoWithDisplayInfo(Anexo anexoData, ImageSource thumbnailSource)
    {
        AnexoData = anexoData;
        ThumbnailSource = thumbnailSource;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public partial class PedidosFaltas : ContentPage
{
    private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
    private readonly int id_colaborador;
    private readonly string token;
    private readonly string nome_abreviado;
    public const string OpcaoDefinicoes = "Definições";
    public const string OpcaoSair = "Sair da app";
    public const string OpcaoCancelar = "Cancelar";

    private List<Anexo> anexosParaEnviar = new List<Anexo>();
    public ObservableCollection<AnexoWithDisplayInfo> AnexosVisiveis { get; } = new ObservableCollection<AnexoWithDisplayInfo>();
    private List<string> _extensoesPermitidas;

    public PedidosFaltas(int IdColaborador, string Token, string NomeAbreviado)
    {
        InitializeComponent();
        BindingContext = this;

        id_colaborador = IdColaborador;
        token = Token;
        nome_abreviado = NomeAbreviado;

        // Carrega as extensões imediatamente
        Task.Run(async () => {
            await CarregarExtensoesPermitidasAsync();
            await InicializarAsync();
        });

        UpdateAttachedFilesLabelVisibility();
    }

    private async Task InicializarAsync()
    {
        InicializarCamposEscondidos();
        List<TipoFalta> tiposDeFaltas = await CarregarTiposFaltasAsync();
        if (PickerFaltas != null)
        {
            PickerFaltas.ItemsSource = tiposDeFaltas;
        }
        else
        {
            await DisplayAlert("Erro de UI", "PickerFaltas não encontrado na página.", "OK");
        }
    }

    private void InicializarCamposEscondidos()
    {
        DatePickerDataFim.IsVisible = false;
        TimePickerInicio.IsVisible = false;
        TimePickerFim.IsVisible = false;
        EntryLocalidade.IsVisible = false;
        lblLoc.IsVisible = false;
        lblDataFim.IsVisible = false;
        FrameLocalidade.IsVisible = false;
        FrameDataFim.IsVisible = false;
        FrameHoraInicio.IsVisible = false;
        FrameHoraFim.IsVisible = false;
        lblhora.IsVisible = false;
        FrameData.IsVisible = false;
        DatePickerData.IsVisible = false;
        lblData.IsVisible = false;
    }

    private async Task<List<TipoFalta>> CarregarTiposFaltasAsync()
    {
        try
        {
            var resposta = await _service.GetTiposFaltasAsync(id_colaborador, token);
            var tiposDeFaltas = resposta.Body.GetTiposFaltasResult?.aTiposFaltas?.ToList();
            return tiposDeFaltas ?? new List<TipoFalta>();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Falha ao carregar os tipos de faltas: " + ex.Message, "OK");
            return new List<TipoFalta>();
        }
    }

    private async Task CarregarExtensoesPermitidasAsync()
    {
        System.Diagnostics.Debug.WriteLine("🔄 A carregar extensões permitidas...");

        try
        {
            var resposta = await _service.GetExtensoesPermitidasAsync(id_colaborador, token);
            var extensoes = resposta?.Body?.GetExtensoesPermitidasResult?.extensoesValidas;

            if (!string.IsNullOrWhiteSpace(extensoes))
            {
                // Primeiro separa por vírgulas e depois por ponto-e-vírgulas
                _extensoesPermitidas = extensoes
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim().ToLowerInvariant().TrimStart('.'))
                    .Where(e => !string.IsNullOrEmpty(e))
                    .Distinct()
                    .ToList();

                System.Diagnostics.Debug.WriteLine("✅ Extensões carregadas da API:");
                foreach (var ext in _extensoesPermitidas)
                    System.Diagnostics.Debug.WriteLine($" - {ext}");
            }
            else
            {
                AplicarExtensoesPadrao("⚠️ Web Service retornou vazio");
            }
        }
        catch (Exception ex)
        {
            AplicarExtensoesPadrao($"❌ Erro ao obter extensões: {ex.Message}");
        }
    }

    private void AplicarExtensoesPadrao(string motivo)
    {
        _extensoesPermitidas = new List<string> { "pdf", "doc", "docx", "jpg", "jpeg", "png" };
        System.Diagnostics.Debug.WriteLine($"[FALLBACK] Usando extensões padrão. Motivo: {motivo}");

#if DEBUG
        MainThread.BeginInvokeOnMainThread(async () => {
            await DisplayAlert("Aviso", $"Usando extensões padrão. Motivo: {motivo}", "OK");
        });
#endif
    }

    private void PickerFaltas_SelectionChanged(object sender, Syncfusion.Maui.Inputs.SelectionChangedEventArgs e)
    {
        if (PickerFaltas.SelectedItem is TipoFalta tipoDeFaltas)
        {
            AtualizarVisibilidadeCampos(tipoDeFaltas.contaMeioDia, tipoDeFaltas.contaHoras, tipoDeFaltas.contaDia, tipoDeFaltas.localidadeOcorrencia);
        }
        else
        {
            InicializarCamposEscondidos();
        }
    }

    private void AtualizarVisibilidadeCampos(bool ContaMeiodia, bool ContaHora, bool dia, bool localidade)
    {
        bool mostrarHoras = ContaHora || ContaMeiodia;
        bool mostrarlocalidade = localidade;

        lblData.IsVisible = dia;
        DatePickerData.IsVisible = dia;
        FrameData.IsVisible = dia;

        lblDataFim.IsVisible = dia;
        DatePickerDataFim.IsVisible = dia;
        FrameDataFim.IsVisible = dia;

        lblhora.IsVisible = mostrarHoras;
        FrameHoraInicio.IsVisible = mostrarHoras;
        FrameHoraFim.IsVisible = mostrarHoras;
        TimePickerInicio.IsVisible = mostrarHoras;
        TimePickerFim.IsVisible = mostrarHoras;

        EntryLocalidade.IsVisible = mostrarlocalidade;
        lblLoc.IsVisible = mostrarlocalidade;
        FrameLocalidade.IsVisible = mostrarlocalidade;

        if (ContaHora == true && dia == true)
        {
            lblDataFim.IsVisible = false;
            DatePickerDataFim.IsVisible = false;
            FrameDataFim.IsVisible = false;
        }
    }

    private async void BtnEnviarFaltas_Clicked(object sender, EventArgs e)
    {
        TipoFalta tipoDeFaltas = null;

        if (PickerFaltas.SelectedItem is TipoFalta selectedTipoDeFalta)
        {
            tipoDeFaltas = selectedTipoDeFalta;
        }
        else
        {
            await DisplayAlert("Erro", "Selecione um tipo de falta válido.", "OK");
            return;
        }

        if (tipoDeFaltas.validaAnexo && anexosParaEnviar.Count == 0)
        {
            string mensagemAnexo = string.IsNullOrEmpty(tipoDeFaltas.anexosNecessarios)
                                  ? "Este tipo de falta requer um ou mais anexos."
                                  : $"Este tipo de falta requer os seguintes anexos: {tipoDeFaltas.anexosNecessarios}.";
            await DisplayAlert("Anexo Necessário", mensagemAnexo, "OK");
            return;
        }

        string obs = EntryObs.Text;
        string data = DatePickerData.Date.ToString("dd/MM/yyyy");
        string datafim = DatePickerDataFim.Date.ToString("dd/MM/yyyy");
        string localidadeOcorrencia = EntryLocalidade.Text;
        string horaInicio = TimePickerInicio.Time.ToString(@"hh\:mm");
        string horaFim = TimePickerFim.Time.ToString(@"hh\:mm");

        var anexosParaServico = new Anexos
        {
            aAnexos = anexosParaEnviar.ToArray()
        };

        await EnviarFaltasAsync(id_colaborador, token, tipoDeFaltas.idTipoFalta, data, datafim, horaInicio, horaFim, localidadeOcorrencia, obs, anexosParaServico);
    }

    private async Task EnviarFaltasAsync(int idColaborador, string token, int idTipoFalta, string dataInicio, string dataFim, string horaInicio, string horaFim, string localidadeOcorrencia, string obs, Anexos anexos)
    {
        try
        {
            var resposta = await _service.SetFaltaAsync(idColaborador, token, idTipoFalta, dataInicio, dataFim, horaInicio, horaFim, localidadeOcorrencia, obs, anexos);
            var result = resposta.Body.SetFaltaResult;

            if (result.erro != 0)
            {
                await DisplayAlert("Erro ao enviar", result.erroMensagem, "OK");
                return;
            }

            await DisplayAlert("Sucesso", "Falta enviada com sucesso!", "OK");

            anexosParaEnviar.Clear();
            AnexosVisiveis.Clear();
            UpdateAttachedFilesLabelVisibility();

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao enviar: {ex.Message}", "OK");
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        string acao = await DisplayActionSheet("Menu de Opções", OpcaoCancelar, null, OpcaoDefinicoes, OpcaoSair);
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

    private string GetIconForFile(string filename)
    {
        string extension = Path.GetExtension(filename).ToLowerInvariant();
        switch (extension)
        {
            case ".pdf":
                return "pdf_icon.png";
            case ".doc":
            case ".docx":
                return "doc_icon.png";
            default:
                return "file_icon.png";
        }
    }

    private async Task AddAnexoToList(FileResult fileResult, bool checkExtension = true)
    {
        if (fileResult == null) return;

        string fileExtension = Path.GetExtension(fileResult.FileName).TrimStart('.').ToLowerInvariant();
        string fileName = fileResult.FileName;

        System.Diagnostics.Debug.WriteLine($"Verificando anexo: '{fileName}'");
        System.Diagnostics.Debug.WriteLine($"Extensão extraída: '{fileExtension}'");

        if (checkExtension)
        {
            System.Diagnostics.Debug.WriteLine("Verificação de extensão ativada");

            if (_extensoesPermitidas == null || !_extensoesPermitidas.Any())
            {
                System.Diagnostics.Debug.WriteLine("Lista de extensões permitidas está vazia ou nula");
                AplicarExtensoesPadrao("Lista de extensões vazia durante verificação");
            }

            System.Diagnostics.Debug.WriteLine($"Extensões permitidas: {string.Join(", ", _extensoesPermitidas)}");

            bool extensaoPermitida = _extensoesPermitidas.Contains(fileExtension);

            System.Diagnostics.Debug.WriteLine($"Extensão permitida? {extensaoPermitida}");

            if (!extensaoPermitida)
            {
                string extensoesFormatadas = string.Join(", ", _extensoesPermitidas.Select(e => $".{e}"));
                string mensagem = $"O tipo de arquivo '.{fileExtension}' não é permitido.\n\nExtensões permitidas: {extensoesFormatadas}";
                await DisplayAlert("Extensão Não Permitida", mensagem, "OK");
                System.Diagnostics.Debug.WriteLine($"**BLOQUEADO**: {mensagem}");
                return;
            }
        }

        try
        {
            using var stream = await fileResult.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            var anexoParaServico = new Anexo
            {
                nomeFicheiro = fileResult.FileName,
                aBytes = fileBytes
            };
            anexosParaEnviar.Add(anexoParaServico);

            ImageSource thumbnail;
            string originalExtensionWithDot = Path.GetExtension(fileResult.FileName).ToLowerInvariant();
            if (originalExtensionWithDot == ".jpg" || originalExtensionWithDot == ".jpeg" || originalExtensionWithDot == ".png" || originalExtensionWithDot == ".gif")
            {
                thumbnail = ImageSource.FromStream(() => new MemoryStream(fileBytes));
            }
            else
            {
                thumbnail = ImageSource.FromFile(GetIconForFile(fileResult.FileName));
            }

            var anexoDisplay = new AnexoWithDisplayInfo(anexoParaServico, thumbnail);
            anexoDisplay.RemoveCommand = new Command(() =>
            {
                AnexosVisiveis.Remove(anexoDisplay);
                anexosParaEnviar.Remove(anexoParaServico);
                UpdateAttachedFilesLabelVisibility();
            });

            AnexosVisiveis.Add(anexoDisplay);
            UpdateAttachedFilesLabelVisibility();
            System.Diagnostics.Debug.WriteLine($"Anexo '{fileResult.FileName}' adicionado com sucesso.");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro ao Anexar", $"Não foi possível anexar o ficheiro: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"Erro ao anexar ficheiro '{fileResult.FileName}': {ex.Message}");
        }
    }

    private void UpdateAttachedFilesLabelVisibility()
    {
        LabelFicheirosAnexados.IsVisible = AnexosVisiveis.Any();
    }

    private async void btnCamera_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await DisplayAlert("Não Suportado", "A captura de fotos não é suportada neste dispositivo.", "OK");
                return;
            }
            FileResult photo = await MediaPicker.Default.CapturePhotoAsync();
            await AddAnexoToList(photo, false); // Não verificar extensão para fotos
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro de Câmara", $"Ocorreu um erro ao tirar a foto: {ex.Message}", "OK");
        }
    }

    private async void btnGallery_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            FileResult photo = await MediaPicker.Default.PickPhotoAsync();
            await AddAnexoToList(photo, false); // Não verificar extensão para fotos da galeria
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro de Galeria", $"Ocorreu um erro ao selecionar a foto: {ex.Message}", "OK");
        }
    }

    private async void btnDocuments_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            FileResult result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Selecione um ficheiro..."
            });
            await AddAnexoToList(result); // Verificar extensão (padrão true)
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro de Documentos", $"Ocorreu um erro ao selecionar o ficheiro: {ex.Message}", "OK");
        }
    }
}