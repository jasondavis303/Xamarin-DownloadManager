using System;
using System.Collections.Generic;
using System.ComponentModel;
using Foundation;
using Plugins.DownloadManager.Interfaces;

namespace Plugins.DownloadManager.iOS
{
    public class DownloadFile : IDownloadFile
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public NSUrlSessionTask Task { get; set; }

        public string Url { get; }

        private string _destinationPathName;
        public string DestinationPathName
        {
            get => _destinationPathName;
            set
            {
                if (Equals(_destinationPathName, value))
                    return;
                _destinationPathName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DestinationPathName)));
            }
        }

        public IDictionary<string, string> Headers { get; }

        private DownloadFileStatus _status;
        public DownloadFileStatus Status
        {
            get => _status;
            set
            {
                if (Equals(_status, value))
                    return;
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        private string _statusDetails;
        public string StatusDetails
        {
            get => _statusDetails;
            set
            {
                if (Equals(_statusDetails, value)) 
                    return;
                _statusDetails = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusDetails)));
            }
        }

        private long _totalBytesExpected;
        public long TotalBytesExpected
        {
            get => _totalBytesExpected;
            set
            {
                if (Equals(_totalBytesExpected, value))
                    return;
                _totalBytesExpected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalBytesExpected)));
            }
        }

        private long _totalBytesWritten;
        public long TotalBytesWritten
        {
            get => _totalBytesWritten;
            set
            {
                if (Equals(_totalBytesWritten, value))
                    return;
                _totalBytesWritten = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalBytesWritten)));
            }
        }

        public string LocalPath => DownloadManager.Current.GetLocalPath(Url);

        public DownloadFile(string url, IDictionary<string, string> headers)
        {
            Url = url;
            Headers = headers;
            Status = DownloadFileStatus.INITIALIZED;
        }

        
        public DownloadFile(NSUrlSessionTask task)
        {
            Url = task.OriginalRequest.Url.AbsoluteString;
            Headers = new Dictionary<string, string>();

            foreach (var header in task.OriginalRequest.Headers)
                Headers.Add(new KeyValuePair<string, string>(header.Key.ToString(), header.Value.ToString()));
        
            switch (task.State)
            {
                case NSUrlSessionTaskState.Running:
                    Status = DownloadFileStatus.RUNNING;
                    break;

                case NSUrlSessionTaskState.Completed:
                    Status = DownloadFileStatus.COMPLETED;
                    break;

                case NSUrlSessionTaskState.Canceling:
                    Status = DownloadFileStatus.RUNNING;
                    break;

                case NSUrlSessionTaskState.Suspended:
                    Status = DownloadFileStatus.PAUSED;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            Task = task;
        }

        public void StartDownload(NSUrlSession session, bool allowsCellularAccess)
        {
            using (var downloadUrl = NSUrl.FromString(Url))
            using (var request = new NSMutableUrlRequest(downloadUrl))
            {
                if (Headers != null)
                {
                    var headers = new NSMutableDictionary();
                    foreach (var header in Headers)
                        headers.SetValueForKey(new NSString(header.Value), new NSString(header.Key));
                    request.Headers = headers;
                }
                request.AllowsCellularAccess = allowsCellularAccess;
                Task = session.CreateDownloadTask(request);
                Task.Resume();
            }
        }
    }
}
