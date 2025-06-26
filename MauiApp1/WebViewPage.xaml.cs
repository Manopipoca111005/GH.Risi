namespace MauiApp1
{
    public partial class WebViewPage : ContentPage
    {
        public WebViewPage(string url)
        {
            InitializeComponent();
            MyWebView.Source = url;
        }

        private async void BackButton_Clicked(object sender, System.EventArgs e)
        {
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
            }
        }
    }
}