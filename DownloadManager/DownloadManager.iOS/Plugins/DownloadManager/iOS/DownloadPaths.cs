using Plugins.DownloadManager.Interfaces;
using System;
using System.IO;

namespace Plugins.DownloadManager.iOS
{
    public class DownloadPaths : IDownloadPaths
    {
        public string DownloadDirectory
        {
            get
            {
                string ret = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library", "Caches", "Downloaded");
                Directory.CreateDirectory(ret);
                return ret;
            }
        }

        public string TempDirectory
        {
            get
            {
                string ret = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..", "Library", "tmp");
                Directory.CreateDirectory(ret);
                return ret;
            }
        }

        public string GetTempPath(string url)
        {
            Uri uri = new Uri(url);
            string ret = Path.Combine(TempDirectory, $"{uri.DnsSafeHost}{uri.LocalPath}");
            Directory.CreateDirectory(Path.GetFileName(ret));
            return ret;
        }

        public string GetLocalPath(string url)
        {
            Uri uri = new Uri(url);
            string ret = Path.Combine(DownloadDirectory, $"{uri.DnsSafeHost}{uri.LocalPath}");
            Directory.CreateDirectory(Path.GetFileName(ret));
            return ret;
        }

        public void MoveFile(string url)
        {
            string local = GetLocalPath(url);
            if (File.Exists(local))
                File.Delete(local);

            File.Move(GetTempPath(url), local);
        }
    }
}