using System;
using System.IO;
using System.Linq;

namespace FrameworkSupport
{
    public class FileDetectionHelpers
    {
        private static readonly byte[] GIF = { 71, 73, 70, 56 };
        private static readonly byte[] ICO = { 0, 0, 1, 0 };
        private static readonly byte[] JPG = { 255, 216, 255 };
        private static readonly byte[] PDF = { 37, 80, 68, 70, 45, 49, 46 };
        private static readonly byte[] PNG = { 137, 80, 78, 71, 13, 10, 26, 10, 0, 0, 0, 13, 73, 72, 68, 82 };
        private static readonly byte[] RIFF = { 82, 73, 70, 70 };
        private static readonly byte[] WEBP = { 87, 69, 66, 80 };

        //other types
        private static readonly byte[] BMP = { 66, 77 };

        private static readonly byte[] DOC = { 208, 207, 17, 224, 161, 177, 26, 225 };
        private static readonly byte[] EXE_DLL = { 77, 90 };
        private static readonly byte[] MP3 = { 255, 251, 48 };
        private static readonly byte[] OGG = { 79, 103, 103, 83, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static readonly byte[] RAR = { 82, 97, 114, 33, 26, 7, 0 };
        private static readonly byte[] SWF = { 70, 87, 83 };
        private static readonly byte[] TIFF = { 73, 73, 42, 0 };
        private static readonly byte[] TORRENT = { 100, 56, 58, 97, 110, 110, 111, 117, 110, 99, 101 };
        private static readonly byte[] TTF = { 0, 1, 0, 0, 0 };
        private static readonly byte[] WAV_AVI = { 82, 73, 70, 70 };
        private static readonly byte[] WMV_WMA = { 48, 38, 178, 117, 142, 102, 207, 17, 166, 217, 0, 170, 0, 98, 206, 108 };
        private static readonly byte[] ZIP_DOCX = { 80, 75, 3, 4 };

        /// <summary>
        /// Checks if an image is valid (on the context of Imageflow/ImageResizer)
        /// </summary>
        /// <param name="stream">The file stream</param>
        /// <param name="fileName">The file name</param>
        /// <returns>A boolean indicating validity and a string with the actual MIME type</returns>
        public static (bool, string) IsValidImageType(Stream stream, string fileName)
        {
            byte[] fileBytes = StreamToByteArray(stream);
            return IsValidImageType(fileBytes, fileName);
        }

        /// <summary>
        /// Checks if an image is valid (on the context of Imageflow/ImageResizer)
        /// </summary>
        /// <param name="file">The file byte array</param>
        /// <param name="fileName">The file name</param>
        /// <returns>A boolean indicating validity and a string with the actual MIME type</returns>
        public static (bool, string) IsValidImageType(byte[] file, string fileName)
        {
            string mime = "application/octet-stream"; //DEFAULT UNKNOWN MIME TYPE
            bool isValid = false;

            //Ensure that the filename isn't empty or null
            if (file is null)
            {
                return (isValid, string.Empty);
            }

            if (file.Take(4).SequenceEqual(GIF))
            {
                isValid = true;
                mime = "image/gif";
            }
            else if (file.Take(3).SequenceEqual(JPG))
            {
                isValid = true;
                mime = "image/jpeg";
            }
            else if (file.Take(16).SequenceEqual(PNG))
            {
                isValid = true;
                mime = "image/png";
            }
            else if (file.Take(4).SequenceEqual(RIFF) && file.Skip(8).Take(4).SequenceEqual(WEBP))
            {
                isValid = true;
                mime = "image/webp";
            }
            else
            {
                mime = TrytoGetMimeType(file, fileName);
            }

            return (isValid, mime);
        }

        /// <summary>
        /// Checks if a PDF is valid
        /// </summary>
        /// <param name="file">The file byte array</param>
        /// <returns>A boolean indicating validity</returns>
        public static bool IsValidPdfType(byte[] file)
        {
            return file.Take(7).SequenceEqual(PDF);
        }

        /// <summary>
        /// Tries to get the actual MIME type of a file based on a few known header signatures.
        /// </summary>
        /// <param name="file">The file byte array</param>
        /// <param name="fileName">The file name</param>
        /// <returns>A string with the actual MIME type or '"application/octet-stream' if unknown</returns>
        public static string TrytoGetMimeType(byte[] file, string fileName)
        {
            string mime = "application/octet-stream"; //DEFAULT UNKNOWN MIME TYPE

            //Ensure that the filename isn't empty or null
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return mime;
            }

            //Get the file extension
            string extension = Path.GetExtension(fileName) == null
                                   ? string.Empty
                                   : Path.GetExtension(fileName).ToUpper();

            //Get the MIME Type
            if (file.Take(2).SequenceEqual(BMP))
            {
                mime = "image/bmp";
            }
            else if (file.Take(8).SequenceEqual(DOC))
            {
                mime = "application/msword";
            }
            else if (file.Take(2).SequenceEqual(EXE_DLL))
            {
                mime = "application/x-msdownload"; //both use same mime type
            }
            else if (file.Take(4).SequenceEqual(GIF))
            {
                mime = "image/gif";
            }
            else if (file.Take(4).SequenceEqual(ICO))
            {
                mime = "image/x-icon";
            }
            else if (file.Take(3).SequenceEqual(JPG))
            {
                mime = "image/jpeg";
            }
            else if (file.Take(3).SequenceEqual(MP3))
            {
                mime = "audio/mpeg";
            }
            else if (file.Take(14).SequenceEqual(OGG))
            {
                if (extension == ".OGX")
                {
                    mime = "application/ogg";
                }
                else
                {
                    mime = extension == ".OGA" ? "audio/ogg" : "video/ogg";
                }
            }
            else if (file.Take(7).SequenceEqual(PDF))
            {
                mime = "application/pdf";
            }
            else if (file.Take(16).SequenceEqual(PNG))
            {
                mime = "image/png";
            }
            else if (file.Take(7).SequenceEqual(RAR))
            {
                mime = "application/x-rar-compressed";
            }
            else if (file.Take(3).SequenceEqual(SWF))
            {
                mime = "application/x-shockwave-flash";
            }
            else if (file.Take(4).SequenceEqual(TIFF))
            {
                mime = "image/tiff";
            }
            else if (file.Take(11).SequenceEqual(TORRENT))
            {
                mime = "application/x-bittorrent";
            }
            else if (file.Take(5).SequenceEqual(TTF))
            {
                mime = "application/x-font-ttf";
            }
            else if (file.Take(4).SequenceEqual(WAV_AVI))
            {
                mime = extension == ".AVI" ? "video/x-msvideo" : "audio/x-wav";
            }
            else if (file.Take(16).SequenceEqual(WMV_WMA))
            {
                mime = extension == ".WMA" ? "audio/x-ms-wma" : "video/x-ms-wmv";
            }
            else if (file.Take(4).SequenceEqual(ZIP_DOCX))
            {
                mime = extension == ".DOCX" ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document" : "application/x-zip-compressed";
            }

            return mime;
        }

        /// <summary>
        /// Converts a strem to a byte array
        /// </summary>
        /// <param name="stream">Stream to convert</param>
        /// <returns>A byte array</returns>
        public static byte[] StreamToByteArray(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}