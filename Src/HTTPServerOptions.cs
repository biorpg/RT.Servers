﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Servers
{
    public class HTTPServerOptions
    {
        /// <summary>
        /// Timeout in milliseconds for idle connections. Set to 0 for no timeout. Default is 10000 (10 seconds).
        /// </summary>
        public int IdleTimeout = 10000;

        /// <summary>
        /// Maximum allowed size for the headers of a request, in bytes. Default is 256 KB.
        /// </summary>
        public int MaxSizeHeaders = 256 * 1024;

        /// <summary>
        /// Maximum allowed size for the content of a POST request, in bytes. Default is 1 GB.
        /// </summary>
        public long MaxSizePostContent = 1024 * 1024 * 1024;

        /// <summary>
        /// The minimum size of a POST request at which the server will store the content of the request in a file instead of in memory. Default is 1 MB.
        /// </summary>
        public long UseFileUploadAtSize = 1024 * 1024;

        /// <summary>
        /// The temporary directory to use for POST requests and file uploads. Default is Path.GetTempPath().
        /// </summary>
        public string TempDir = Path.GetTempPath();

        /// <summary>
        /// Maps from file extension to MIME type. Used by FileSystemHandler().
        /// Use the key "*" for the default MIME type. Otherwise the default is "application/octet-stream".
        /// </summary>
        public Dictionary<string, string> MIMETypes = new Dictionary<string, string>();

        /// <summary>
        /// Content-Type to return when handler provides none. Default is "text/html; charset=utf-8".
        /// </summary>
        public string DefaultContentType = "text/html; charset=utf-8";

        /// <summary>
        /// Enum specifying which way directory listings should be generated.
        /// </summary>
        public DirectoryListingStyle DirectoryListingStyle = DirectoryListingStyle.XMLplusXSL;

        public HTTPServerOptions()
        {
            // Plain text
            MIMETypes["txt"] = "text/plain; charset=utf-8";
            MIMETypes["csv"] = "text/csv; charset=utf-8";

            // HTML and dependancies
            MIMETypes["htm"] = "text/html; charset=utf-8";
            MIMETypes["html"] = "text/html; charset=utf-8";
            MIMETypes["css"] = "text/css; charset=utf-8";
            MIMETypes["js"] = "text/javascript; charset=utf-8";

            // XML and stuff
            MIMETypes["xhtml"] = "application/xhtml+xml; charset=utf-8";
            MIMETypes["xml"] = "application/xml; charset=utf-8";
            MIMETypes["xsl"] = "application/xml; charset=utf-8";

            // Images
            MIMETypes["gif"] = "image/gif";
            MIMETypes["png"] = "image/png";
            MIMETypes["jp2"] = "image/jp2";
            MIMETypes["jpg"] = "image/jpeg";
            MIMETypes["jpeg"] = "image/jpeg";
            MIMETypes["bmp"] = "image/bmp";

            // Default
            MIMETypes["*"] = "application/octet-stream";
        }
    }
}
