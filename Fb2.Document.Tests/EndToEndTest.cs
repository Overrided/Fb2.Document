using System;
using System.IO;
using System.Linq;
using Fb2.Document.Models;
using Fb2.Document.Tests.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fb2.Document.Tests
{
    [TestClass]
    public class EndToEndTest : BaseTest
    {
        string GeneratedFolderPath = null;

        [TestInitialize]
        public void SetUp()
        {
            var projectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;

            GeneratedFolderPath = Path.Combine(projectPath, "Generated");

            if (!Directory.Exists(GeneratedFolderPath))
                Directory.CreateDirectory(GeneratedFolderPath);
            else
                CleanupFolder(GeneratedFolderPath);
        }

        [TestMethod]
        public void CreateSaveLoad()
        {
            var document = Fb2Document.CreateDocument();

            var description = new BookDescription();

            #region Title Info

            var titleInfo = new TitleInfo();

            var genre = InstantiateAndLoad<BookGenre>("hardcore science fiction");

            titleInfo.Content.Add(genre);

            var author = new Author();

            var firstName = InstantiateAndLoad<FirstName>("Denys");

            var middleName = InstantiateAndLoad<MiddleName>("Over");

            var lastName = InstantiateAndLoad<LastName>("Pukhkyi");

            var nickName = InstantiateAndLoad<Nickname>("Overrided");

            author.Content.Add(firstName);
            author.Content.Add(middleName);
            author.Content.Add(lastName);
            author.Content.Add(nickName);

            titleInfo.Content.Add(author);

            var bookTitle = InstantiateAndLoad<BookTitle>("Test Book Title");

            titleInfo.Content.Add(bookTitle);

            #endregion

            #region Publish info

            var publishInfo = new PublishInfo();

            var bookName = InstantiateAndLoad<BookName>("Test Book Name");

            publishInfo.Content.Add(bookName);

            #endregion

            description.Content.Add(titleInfo);
            description.Content.Add(publishInfo);

            document.Book.Content.Add(description);

            var body = new BookBody();

            var bodyTitle = new Title();

            var titleParagraph = GetParagraph();

            bodyTitle.Content.Add(titleParagraph);

            var contentParagraph = GetParagraph("test paragraph2 text ");

            body.Content.Add(bodyTitle);
            body.Content.Add(contentParagraph);

            document.Book.Content.Add(body);

            var xDoc = document.ToXml();
            var gen1FilePath = Path.Combine(GeneratedFolderPath, "gen1.fb2"); // saving initial file
            xDoc.Save(gen1FilePath);

            var gen2Document = new Fb2Document();

            using (var gen1ContentStream = File.OpenRead(gen1FilePath))
            {
                gen2Document.Load(gen1ContentStream); // loading file
            }

            Assert.IsTrue(gen2Document.IsLoaded);

            var xDoc2 = gen2Document.ToXml();
            var gen2FilePath = Path.Combine(GeneratedFolderPath, "gen2.fb2");
            xDoc2.Save(gen2FilePath); // saving again

            // ok, now the funniest part

            var gen1ByteContent = File.ReadAllBytes(gen1FilePath);
            var gen2ByteContent = File.ReadAllBytes(gen2FilePath);

            CollectionAssert.AreEqual(gen1ByteContent, gen2ByteContent);

            // lets double check, just in case

            var gen1StringContent = File.ReadAllText(gen1FilePath);
            var gen2StringContent = File.ReadAllText(gen2FilePath);

            Assert.AreEqual(gen1StringContent, gen2StringContent);
        }

        private Paragraph GetParagraph(string text = null)
        {
            var parag = new Paragraph();

            var testText = InstantiateAndLoad<TextItem>(text ?? "test paragraph text ");

            var strong = new Strong();
            var strongText = InstantiateAndLoad<TextItem>("test strong ");

            strong.Content.Add(strongText);

            var italic = new Emphasis();
            var italicText = InstantiateAndLoad<TextItem>("test italic ");

            italic.Content.Add(italicText);

            parag.Content.Add(testText);
            parag.Content.Add(strong);
            parag.Content.Add(italic);

            return parag;
        }

        private void CleanupFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return;

            var files = Directory.EnumerateFiles(folderPath).ToList();

            if (!files.Any())
                return;

            Console.WriteLine("Generated folder cleanup  - start");

            foreach (var file in files)
            {
                File.Delete(file);
            }

            Console.WriteLine("Generated folder cleanup  - done");
        }
    }
}
