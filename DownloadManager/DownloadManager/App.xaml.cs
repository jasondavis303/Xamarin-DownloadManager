using Plugins.DownloadManager.Interfaces;
using Xamarin.Forms;

namespace DownloadManager
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        public static IDownloadManager DownloadManager { get; set; }
    }
}
