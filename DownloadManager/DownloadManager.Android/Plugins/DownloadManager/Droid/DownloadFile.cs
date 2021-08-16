using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.App;
using Android.Database;
using Plugins.DownloadManager.Interfaces;
using Uri = Android.Net.Uri;

namespace Plugins.DownloadManager.Droid
{
    public class DownloadFile : IDownloadFile
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DownloadFile(string url, IDictionary<string, string> headers)
        {
            Url = url;
            Headers = headers;
            Status = DownloadFileStatus.INITIALIZED;
        }

        public long Id { get; set; }

        public string Url { get; set; }

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

        /**
         * Reinitializing an object after the app restarted
         */
        public DownloadFile(ICursor cursor)
        {
            Id = cursor.GetLong(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnId));
            Url = cursor.GetString(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnUri));

            Status = (DownloadStatus)cursor.GetInt(cursor.GetColumnIndex(Android.App.DownloadManager.ColumnStatus)) switch
            {
                DownloadStatus.Failed => DownloadFileStatus.FAILED,
                DownloadStatus.Paused => DownloadFileStatus.PAUSED,
                DownloadStatus.Pending => DownloadFileStatus.PENDING,
                DownloadStatus.Running => DownloadFileStatus.RUNNING,
                DownloadStatus.Successful => DownloadFileStatus.COMPLETED,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public void StartDownload(Android.App.DownloadManager downloadManager, string destinationPathName, bool allowedOverMetered, DownloadVisibility notificationVisibility)
        {
            using var downloadUrl = Uri.Parse(Url);
            using var request = new Android.App.DownloadManager.Request(downloadUrl);
            if (Headers != null)
                foreach (var header in Headers)
                    request.AddRequestHeader(header.Key, header.Value);

            if (destinationPathName != null)
            {
                var file = new Java.IO.File(destinationPathName);
                request.SetDestinationUri(Uri.FromFile(file));

                if (file.Exists())
                    file.Delete();
            }

            //request.SetVisibleInDownloadsUi(isVisibleInDownloadsUi);
            request.SetAllowedOverMetered(allowedOverMetered);
            request.SetNotificationVisibility(notificationVisibility);
            Id = downloadManager.Enqueue(request);
        }
    }
}
