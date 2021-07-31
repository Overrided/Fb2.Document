using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.IntegrationTests
{
    public class Fb2DocumentTests
    {
        private const string SamplesFolderName = "Samples";
        private const string SampleFileName = "_Test_1.fb2";

        // TODO : add tests for all available Load methods:
        // string, XDocument, synchronous stream etc.

        [Fact]
        public async Task InstancesOfBookAreSame()
        {
            var sampleFileInfo = GetSampleFileInfo();

            if (!sampleFileInfo.Exists)
                throw new Exception("Sample file does not exist");

            using (var fileReadStream = sampleFileInfo.OpenRead())
            {
                var firstDocument = new Fb2Document();
                await firstDocument.LoadAsync(fileReadStream);

                RewindStream(fileReadStream);

                var secondDocument = Fb2Document.CreateDocument();
                await secondDocument.LoadAsync(fileReadStream);

                var firstBook = firstDocument.Book;
                var secondBook = secondDocument.Book;

                firstBook.Should().Be(secondBook);

                fileReadStream.Close();
            }
        }

        private FileInfo GetSampleFileInfo()
        {
            var samplesFolderPath = Path.Combine(Environment.CurrentDirectory, SamplesFolderName);

            if (!Directory.Exists(samplesFolderPath))
                throw new Exception($"{samplesFolderPath} folder does not exist.");

            var filePath = Path.Combine(samplesFolderPath, SampleFileName);
            if (!File.Exists(filePath))
                throw new Exception($"{filePath} file does not exist.");

            var fileInfo = new FileInfo(filePath);
            return fileInfo;
        }

        private void RewindStream(Stream stream)
        {
            if (!stream.CanSeek)
                return;

            stream.Seek(0, SeekOrigin.Begin);
            stream.Position = 0;
        }
    }
}
