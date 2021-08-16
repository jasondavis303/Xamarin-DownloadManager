using Foundation;
using Plugins.DownloadManager.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace Plugins.DownloadManager.iOS
{
    /// <summary>
    /// The iOS implementation of the download manager.
    /// </summary>
    public class DownloadManager : IDownloadManager
    {
        public static readonly DownloadManager Current = new DownloadManager();

        private static readonly UrlSessionDownloadDelegate _sessionDownloadDelegate = new UrlSessionDownloadDelegate();

        private string _identifier => NSBundle.MainBundle.BundleIdentifier + ".BackgroundTransferSession";

        private readonly NSUrlSession _backgroundSession;
        
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public DownloadManager ()
        {
            _queue = new List<IDownloadFile>();
            _sessionDownloadDelegate.Controller = this;

            using (var configuration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(_identifier))
                _backgroundSession = NSUrlSession.FromConfiguration(configuration, _sessionDownloadDelegate, null);

            // Reinitialize tasks that were started before the app was terminated or suspended
            _backgroundSession.GetTasks2((dataTasks, uploadTasks, downloadTasks) =>
            {
                foreach (var task in downloadTasks)
                    AddFile(new DownloadFile(task));
            });
        }

        public static Action BackgroundSessionCompletionHandler { get; set; }

        private readonly IList<IDownloadFile> _queue;
        public IEnumerable<IDownloadFile> Queue
        {
            get
            {
                lock (_queue)
                {
                    return _queue.ToList();
                }
            }
        }

        public Func<IDownloadFile, string> PathNameForDownloadedFile { get; set; }

        public IDownloadFile CreateDownloadFile (string url) => CreateDownloadFile (url, new Dictionary<string, string> ());
        
        public IDownloadFile CreateDownloadFile (string url, IDictionary<string, string> headers) => new DownloadFile (url, headers);

        public void Start(IDownloadFile iDownloadFile, bool mobileNetworkAllowed = false)
        {
            var file = (DownloadFile)iDownloadFile;
            AddFile(file);            
            NSOperationQueue.MainQueue.BeginInvokeOnMainThread(() => file.StartDownload(_backgroundSession, mobileNetworkAllowed));
        }

        public void Abort (IDownloadFile iDownloadFile)
        {
            var file = (DownloadFile)iDownloadFile;
            file.Status = DownloadFileStatus.CANCELED;
            file.Task?.Cancel ();
            RemoveFile(file);
        }

        public void AbortAll()
        {
            foreach (var file in Queue)
                Abort(file);
        }

        protected internal void AddFile(IDownloadFile file)
        {
            lock (_queue) 
            {
                _queue.Add(file);
            }
            CollectionChanged?.Invoke(Queue, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, file));
        }

        protected internal void RemoveFile(IDownloadFile file)
        {
            lock (_queue)
            {
                _queue.Remove(file);
            }
            CollectionChanged?.Invoke(Queue, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, file));
        }



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

        public string GetTempPath(string url) => GetTempPath(new Uri(url));

        public string GetTempPath(Uri uri)
        {
            string ret = Path.Combine(TempDirectory, uri.GetLocalHostPath());
            Directory.CreateDirectory(Path.GetFileName(ret));
            return ret;
        }

        public string GetLocalPath(string url) => GetLocalPath(new Uri(url));

        public string GetLocalPath(Uri uri)
        {
            string ret = Path.Combine(DownloadDirectory, uri.GetLocalHostPath());
            Directory.CreateDirectory(Path.GetFileName(ret));
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
