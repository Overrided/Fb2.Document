using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fb2.Document.Constants;
using Fb2.Document.Models;
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

        [Fact]
        public async Task BookContentCheck()
        {
            var sampleFileInfo = GetSampleFileInfo();

            using (var fileReadStream = sampleFileInfo.OpenRead())
            {
                var document = new Fb2Document();
                await document.LoadAsync(fileReadStream);

                document.Bodies.Should().HaveCount(3);

                var firstBody = document.Bodies[0];
                var firstBodyTitle = firstBody.GetFirstChild<Title>();
                firstBodyTitle.Should().NotBeNull();
                var firstBodySections = firstBody.GetChildren<BodySection>();
                firstBodySections.Should().HaveCount(9);

                var secondBody = document.Bodies[1];
                var secondBodyAttributes = secondBody.Attributes.Should().HaveCount(1);
                var secondBodyNameAttribute = secondBody.Attributes.First();
                secondBodyNameAttribute.Key.Should().Be(AttributeNames.Name);
                secondBodyNameAttribute.Value.Should().Be("notes");

                var secondBodyTitle = secondBody.GetFirstChild<Title>();
                secondBodyTitle.Should().NotBeNull();

                var secondBodySections = secondBody.GetChildren<BodySection>();
                secondBodySections.Should().HaveCount(20);

                var thirdBody = document.Bodies[2];
                var thirdBodySections = thirdBody.GetChildren<BodySection>();
                thirdBodySections.Should().HaveCount(1);

                document.BinaryImages.Should().HaveCount(33);
            }
        }

        [Fact]
        public async Task RewriteContent_StaysSame()
        {
            var sampleFileInfo = GetSampleFileInfo();

            using (var fileReadStream = sampleFileInfo.OpenRead())
            {
                // loading document first time
                var firstDocument = new Fb2Document();
                await firstDocument.LoadAsync(fileReadStream);

                var firstDocXml = firstDocument.ToXml();
                var secondDocument = new Fb2Document();
                secondDocument.Load(firstDocXml);

                var firstBook = firstDocument.Book;
                var secondBook = secondDocument.Book;

                firstBook.Should().Be(secondBook);
            }
        }

        [Fact]
        public async Task LoadWithoutUnsafeNodes_DifferentBooks()
        {
            var sampleFileInfo = GetSampleFileInfo();

            using (var fileReadStream = sampleFileInfo.OpenRead())
            {
                // loading document first time
                var firstDocument = new Fb2Document();
                await firstDocument.LoadAsync(fileReadStream);

                RewindStream(fileReadStream);

                // loading document without unsafe nodes
                var secondDocument = new Fb2Document();
                await secondDocument.LoadAsync(fileReadStream, false);

                var firstBook = firstDocument.Book;
                var secondBook = secondDocument.Book;

                // different content due to skipped unsafe nodes
                firstBook.Should().NotBe(secondBook);
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
            if (!fileInfo.Exists)
                throw new Exception("Sample file does not exist");

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
