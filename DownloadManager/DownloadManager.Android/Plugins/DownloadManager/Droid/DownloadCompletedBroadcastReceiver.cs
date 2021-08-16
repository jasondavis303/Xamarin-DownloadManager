using System.Linq;
using Android.App;
using Android.Content;

namespace Plugins.DownloadManager.Droid
{
    [BroadcastReceiver(Enabled = true, Exported = true), IntentFilter(new[] { Android.App.DownloadManager.ActionDownloadComplete })]
    public class DownloadCompletedBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var reference = intent.GetLongExtra(Android.App.DownloadManager.ExtraDownloadId, -1);

            var downloadFile = DownloadManager.Current.Queue.Cast<DownloadFile>().FirstOrDefault(f => f.Id == reference);
            if (downloadFile == null) 
                return;

            var query = new Android.App.DownloadManager.Query();
            query.SetFilterById(downloadFile.Id);

            try
            {
                using var cursor = ((Android.App.DownloadManager)context.GetSystemService(Context.DownloadService)).InvokeQuery(query);
                while (cursor != null && cursor.MoveToNext())
                {
                    DownloadManager.Current.UpdateFileProperties(cursor, downloadFile);
                }
                cursor?.Close();
            }
            catch (Android.Database.Sqlite.SQLiteException)
            {
                // I lately got an exception that the database was unaccessible ...
            }
        }
    }
}
