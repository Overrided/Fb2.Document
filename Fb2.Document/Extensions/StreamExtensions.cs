using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Fb2.Document.Extensions
{
    public static class StreamExtensions
    {
        public static void SeekZero(this Stream stream)
        {
            if (!stream.CanSeek)
                throw new ArgumentException($"CanSeek is false in {nameof(stream)}!");

            stream.Seek(0, SeekOrigin.Begin);
            stream.Position = 0; // double check
        }

        public static async Task<MemoryStream> CloneAsync(this Stream source)
        {
            if (source.Position != 0)
                source.SeekZero();

            var result = new MemoryStream();
            await source.CopyToAsync(result);

            if (result.Position != 0)
                result.SeekZero();

            return result;
        }

        public static MemoryStream Clone(this Stream source)
        {
            if (source.Position != 0)
                source.SeekZero();

            var result = new MemoryStream();
            source.CopyTo(result);

            if (result.Position != 0)
                result.SeekZero();

            return result;
        }

        public static Encoding GetXmlEncodingOrDefault(this Stream xmlContent, Encoding defaultEncoding)
        {
            if (defaultEncoding == null)
                throw new ArgumentNullException($"{nameof(defaultEncoding)} is null!");

            using (var xmlreader = new XmlTextReader(xmlContent))
            {
                xmlreader.MoveToContent();
                return xmlreader.Encoding ?? defaultEncoding;
            }
        }
    }
}
