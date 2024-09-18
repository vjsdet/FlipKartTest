using NLog;
using System;
using System.IO;
using System.IO.Compression;

namespace FrameworkSupport
{
    public static class StreamHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static string DecompressStream(Stream dataStream)
        {
            try
            {
                using (MemoryStream body = new MemoryStream())
                {
                    //
                    // Apparently, in some universe, disposing of the source stream by default in the GZipStream constructor is a good side effect.  Be sure to leaveOpen.
                    //
                    using (GZipStream compressionStream = new GZipStream(dataStream, CompressionMode.Decompress, true))
                    {
                        compressionStream.CopyTo(body);
                    }

                    body.Seek(0, SeekOrigin.Begin);

                    string bodyString = null;
                    using (StreamReader sr = new StreamReader(body, true))
                    {
                        bodyString = sr.ReadToEnd();
                    }

                    return bodyString;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error trying to decompress stream - {ex.FullExceptionMessage()}");
                return null;
            }
        }
    }
}