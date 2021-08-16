using System;
using System.IO;
using Xamarin.Forms;

namespace DownloadManager
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            string url = "https://s3.us-west-1.wasabisys.com/s3xp-demo-bucket-1/image%201.png";

            string local = App.DownloadPaths.GetLocalPath(url);
            bool exists = File.Exists(local);
            if (!exists)
            {
                var file = App.DownloadManager.CreateDownloadFile(url);
                file.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(file.Status))
                    {
                        if(file.Status == Plugins.DownloadManager.Interfaces.DownloadFileStatus.COMPLETED)
                        {
                            App.DownloadPaths.MoveFile(url);
                        }
                        else if(file.Status == Plugins.DownloadManager.Interfaces.DownloadFileStatus.FAILED)
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
