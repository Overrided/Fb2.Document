using System;
using System.IO;

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
    }
}
