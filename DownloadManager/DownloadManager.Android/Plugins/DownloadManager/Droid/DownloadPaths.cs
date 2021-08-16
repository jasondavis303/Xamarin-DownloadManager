using Android.App;
using Plugins.DownloadManager.Interfaces;
using System;
using System.IO;
using Xamarin.Essentials;

namespace Plugins.DownloadManager.Droid
{
    public class DownloadPaths : IDownloadPaths
    {
        public static readonly DownloadPaths Current = new DownloadPaths();

        private DownloadPaths()
        {
            //Skip media scanner in this app
            using (var fs = File.Create(Path.Combine(TempDirectory, ".nomedia"))) { }
            using (var fs = File.Create(Path.Combine(DownloadDirectory, ".nomedia"))) { }
        }

        private string Root
        {
            get
            {
                if (Android.OS.Environment.MediaMounted.Equals(Android.OS.Environment.ExternalStorageState))
                    return Application.Context.GetExternalFilesDir(null).AbsolutePath;

                return FileSystem.AppDataDirectory;
            }
        }

        public string DownloadDirectory
        {
            get
            {
                string ret = Path.Combine(Root, "Caches");
                Directory.CreateDirectory(ret);
                return ret;
            }
        }

        public string TempDirectory
        {
            get
            {
                string ret = Path.Combine(Root, "tmp");
                Directory.CreateDirectory(ret);
                return ret;
            }
        }

        public string GetTempPath(string url) => GetTempPath(new Uri(url));

        public string GetTempPath(Uri uri)
        {
            string ret = Path.Combine(TempDirectory, uri.GetLocalHostPath());
            Directory.CreateDirectory(Path.GetDirectoryName(ret));
            return ret;
        }

        public string GetLocalPath(string url) => GetLocalPath(new Uri(url));

        public string GetLocalPath(Uri uri)
        {
            string ret = Path.Combine(DownloadDirectory, uri.GetLocalHostPath());
            Directory.CreateDirectory(Path.GetDirectoryName(ret));
            return ret;
        }

        public void MoveFile(string url) => MoveFile(new Uri(url));
        
        public void MoveFile(Uri uri)
        {
            string local = GetLocalPath(uri);
            if (File.Exists(local))
                File.Delete(local);

            File.Move(GetTempPath(uri), local);
        }
    }
}