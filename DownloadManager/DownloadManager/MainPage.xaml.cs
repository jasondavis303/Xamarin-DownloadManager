using System;
using System.IO;
using Xamarin.Forms;
using System.Linq;

namespace DownloadManager
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            string url = "https://s3.us-west-1.wasabisys.com/s3xp-demo-bucket-1/image%201.png";

            string local = App.DownloadManager.GetLocalPath(url);
            bool exists = File.Exists(local);
            if (!exists)
            {
                if (!App.DownloadManager.Queue.Any(item => item.Url == url))
                {
                    var file = App.DownloadManager.CreateDownloadFile(url);
                    file.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == nameof(file.Status))
                        {
                            if (file.Status == Plugins.DownloadManager.Interfaces.DownloadFileStatus.FAILED)
                            {
                                throw new Exception(file.StatusDetails);
                            }
                        }
                    };

                    App.DownloadManager.Start(file);
                }
            }
        }
    }
}
