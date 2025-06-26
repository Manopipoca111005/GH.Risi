using CommunityToolkit.Maui.Views;
using GH_Metodos;

namespace MauiApp1
{
    public partial class SelecioneEntidadePopup : Popup
    {
        public SelecioneEntidadePopup(List<Entidade> entidades)
        {
            InitializeComponent();
            EntidadesList.ItemsSource = entidades;

        }

        private void OnRadioButtonCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                var radio = sender as RadioButton;
                var entidadeSelecionada = radio?.BindingContext as Entidade;


                this.Close(entidadeSelecionada);
            }
        }



    }
}
