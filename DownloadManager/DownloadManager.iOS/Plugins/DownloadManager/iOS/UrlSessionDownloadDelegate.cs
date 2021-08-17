using System.Linq;
using Foundation;
using Plugins.DownloadManager.Interfaces;

namespace Plugins.DownloadManager.iOS
{
    public class UrlSessionDownloadDelegate : NSObject, INSUrlSessionDownloadDelegate
    {
        public static readonly UrlSessionDownloadDelegate Current = new UrlSessionDownloadDelegate();

        private UrlSessionDownloadDelegate() { }

        protected DownloadFile GetDownloadFileByTask(NSUrlSessionTask downloadTask)
        {
            return DownloadManager.Current.Queue
                .Cast<DownloadFile>()
                .FirstOrDefault(
                    i => i.Task != null &&
                    (int)i.Task.TaskIdentifier == (int)downloadTask.TaskIdentifier
                );
        }

        [Export("URLSession:downloadTask:didResumeAtOffset:expectedTotalBytes:")]
        public void DidResume(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long resumeFileOffset, long expectedTotalBytes)
        {
            var file = GetDownloadFileByTask(downloadTask);
            if (file == null)
            {
                downloadTask.Cancel();
                return;
            }

            file.Status = DownloadFileStatus.RUNNING;
        }


        [Export("URLSession:task:didCompleteWithError:")]
        public void DidCompleteWithError(NSUrlSession session, NSUrlSessionTask task, NSError error)
        {
            var file = GetDownloadFileByTask(task);
            if (file == null)
                return;

            file.StatusDetails = error.LocalizedDescription;
            file.Status = DownloadFileStatus.FAILED;
            DownloadManager.Current.RemoveFile(file);
        }


        [Export("URLSession:downloadTask:didWriteData:totalBytesWritten:totalBytesExpectedToWrite:")]
        public void DidWriteData(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long bytesWritten, long totalBytesWritten, long totalBytesExpectedToWrite)
        {
            var file = GetDownloadFileByTask(downloadTask);
            if (file == null)
            {
                downloadTask.Cancel();
                return;
            }

            file.Status = DownloadFileStatus.RUNNING;
            file.TotalBytesExpected = totalBytesExpectedToWrite;
            file.TotalBytesWritten = totalBytesWritten;
        }

        public void DidFinishDownloading(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, NSUrl location)
        {
            var file = GetDownloadFileByTask(downloadTask);
            if (file == null)
            {
                downloadTask.Cancel();
                return;
            }

            // On iOS 9 and later, this method is called even so the response-code is 400 or higher. See https://github.com/cocos2d/cocos2d-x/pull/14683
            var response = downloadTask.Response as NSHttpUrlResponse;
            if (response != null && response.StatusCode >= 400)
            {
                file.StatusDetails = "Error.HttpCode: " + response.StatusCode;
                file.Status = DownloadFileStatus.FAILED;
                DownloadManager.Current.RemoveFile(file);
                return;
            }

            var success = true;
            var destinationPathName = DownloadManager.Current.PathNameForDownloadedFile?.Invoke(file);
            if (destinationPathName != null)
            {
                success = MoveDownloadedFile(file, location, destinationPathName);
                file.DestinationPathName = destinationPathName;
            }
            else
            {
                file.DestinationPathName = location.ToString();
            }

            // If the file destination is unknown or was moved successfully ...
            if (success)
                file.Status = DownloadFileStatus.COMPLETED;

            DownloadManager.Current.RemoveFile(file);
        }

        public bool MoveDownloadedFile(DownloadFile file, NSUrl location, string destinationPathName)
        {
            var fileManager = NSFileManager.DefaultManager;

            var destinationUrl = new NSUrl(destinationPathName, false);
            NSError removeCopy;
            NSError errorCopy;

            fileManager.Remove(destinationUrl, out removeCopy);
            var success = fileManager.Copy(location, destinationUrl, out errorCopy);

            if (!success)
            {
                file.StatusDetails = errorCopy.LocalizedDescription;
                file.Status = DownloadFileStatus.FAILED;
            }

            return success;
        }

        [Export("URLSessionDidFinishEventsForBackgroundURLSession:")]
        public void DidFinishEventsForBackgroundSession(NSUrlSession session)
        {
            var handler = DownloadManager.BackgroundSessionCompletionHandler;
            if (handler != null)
            {
                DownloadManager.BackgroundSessionCompletionHandler = null;
                handler();
            }
        }
    }
}
