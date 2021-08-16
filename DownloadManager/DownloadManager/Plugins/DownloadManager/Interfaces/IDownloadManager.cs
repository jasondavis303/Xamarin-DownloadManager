﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Plugins.DownloadManager.Interfaces
{
    /// <summary>
    /// Download manager.
    /// </summary>
    public interface IDownloadManager
    {
        /// <summary>
        /// Gets the queue holding all the pending and downloading files.
        /// </summary>
        /// <value>The queue.</value>
        IEnumerable<IDownloadFile> Queue { get; }

        /// <summary>
        /// Occurs when the queue changed.
        /// </summary>
        event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Creates a download file.
        /// </summary>
        /// <returns>The download file.</returns>
        /// <param name="url">URL to download.</param>
        IDownloadFile CreateDownloadFile(string url);

        /// <summary>
        /// Creates a download file.
        /// </summary>
        /// <returns>The download file.</returns>
        /// <param name="url">URL to download.</param>
        /// <param name="headers">Headers to send along when requesting the URL.</param>
        IDownloadFile CreateDownloadFile(string url, IDictionary<string, string> headers);

        /// <summary>
        /// Start downloading the file. Most of the systems will put this file into a queue first.
        /// </summary>
        /// <param name="file">File.</param>
        /// <param name="mobileNetworkAllowed">If mobile network is allowed.</param>
        void Start(IDownloadFile file, bool mobileNetworkAllowed = false);

        /// <summary>
        /// Abort downloading the file.
        /// </summary>
        /// <param name="file">File.</param>
        void Abort(IDownloadFile file);

        /// <summary>
        /// Abort all.
        /// </summary>
        /// <returns>void</returns>
        void AbortAll();

        /// <summary>
        /// Directory  where downloads are stored
        /// </summary>
        string DownloadDirectory { get; }

        /// <summary>
        /// Directory where a url will be saved
        /// </summary>
        string GetLocalPath(string url);

        /// <summary>
        /// Directory where a url will be saved
        /// </summary>
        string GetLocalPath(Uri uri);
    }
}
