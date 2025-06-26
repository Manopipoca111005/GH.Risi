using CommunityToolkit.Maui.Views;
using GH_Metodos;
using System.Globalization;

namespace MauiApp1
{
    public partial class SelecionaColaboradoresPopup : Popup
    {
        private readonly Service6SoapClient _service = new Service6SoapClient(Service6SoapClient.EndpointConfiguration.Service6Soap);
        private readonly int idColaborador;
        private readonly string Token;
        private readonly string mesano;
        private readonly string nome_abreviado;
        public SelecionaColaboradoresPopup(List<Colaborador> colaboradores)
        {
            InitializeComponent();
            ColaboradoresList.ItemsSource = colaboradores;


        }

        private void OnRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                var radio = sender as RadioButton;
                var colaboradorSelecionado = radio?.BindingContext as Colaborador;


                this.Close(colaboradorSelecionado);

            }

        }



    }
}
