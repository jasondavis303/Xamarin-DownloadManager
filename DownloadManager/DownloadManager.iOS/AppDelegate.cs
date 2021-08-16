using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Foundation;
using ObjCRuntime;
using UIKit;

namespace DownloadManager.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            App.DownloadManager = Plugins.DownloadManager.iOS.DownloadManager.Current;
            Plugins.DownloadManager.iOS.DownloadManager.Current.PathNameForDownloadedFile = (file) => Plugins.DownloadManager.iOS.DownloadManager.Current.GetLocalPath(file.Url);
            
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        public override void HandleEventsForBackgroundUrl(UIApplication application, string sessionIdentifier, Action completionHandler)
        {
            Plugins.DownloadManager.iOS.DownloadManager.BackgroundSessionCompletionHandler = completionHandler;
        }
    }
}
