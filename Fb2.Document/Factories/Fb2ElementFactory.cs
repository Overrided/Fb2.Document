using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Fb2.Document.Constants;
using Fb2.Document.Models;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Factories
{
    public class Fb2ElementFactory
    {
        private static Fb2ElementFactory instance = null;

        public readonly ReadOnlyDictionary<string, Type> KnownNodes = new ReadOnlyDictionary<string, Type>
            (new Dictionary<string, Type>()
                {
                    { ElementNames.FictionBook, typeof(FictionBook) },
                    { ElementNames.BinaryImage, typeof(BinaryImage) },
                    { ElementNames.Description, typeof(BookDescription) },
                    { ElementNames.TitleInfo, typeof(TitleInfo) },

                    { ElementNames.SrcTitleInfo, typeof(SrcTitleInfo) },
                    { ElementNames.DocumentInfo, typeof(DocumentInfo) },
                    { ElementNames.PublishInfo, typeof(PublishInfo) },
                    { ElementNames.CustomInfo, typeof(CustomInfo)},

                    { ElementNames.Genre, typeof(BookGenre) },
                    { ElementNames.Author, typeof(Author) },

                    { ElementNames.FirstName, typeof(FirstName) },
                    { ElementNames.MiddleName, typeof(MiddleName) },
                    { ElementNames.LastName, typeof(LastName) },
                    { ElementNames.NickName, typeof(Nickname) },
                    { ElementNames.HomePage, typeof(HomePage) },
                    { ElementNames.Email, typeof(Email) },
                    { ElementNames.FictionId, typeof(FictionId) },
                    { ElementNames.Lang, typeof(Lang) },
                    { ElementNames.SrcLang, typeof(SrcLang) },

                    { ElementNames.BookTitle, typeof(BookTitle) },
                    { ElementNames.Annotation, typeof(Annotation) },
                    { ElementNames.Paragraph, typeof(Paragraph) },
                    { ElementNames.FictionText, typeof(TextItem) },
                    { ElementNames.TextStyle, typeof(TextStyle) },
                    { ElementNames.Emphasis, typeof(Emphasis) },
                    { ElementNames.Strong, typeof(Strong) },
                    { ElementNames.TextLink, typeof(TextLink) },
                    { ElementNames.Image, typeof(Image) },
                    { ElementNames.Superscript, typeof(Superscript) },
                    { ElementNames.Subscript, typeof(Subscript) },
                    { ElementNames.Code, typeof(Code) },
                    { ElementNames.Date, typeof(Date) },
                    { ElementNames.Strikethrough, typeof(Strikethrough) },

                    { ElementNames.Table, typeof(Table) },
                    { ElementNames.TableRow, typeof(TableRow) },
                    { ElementNames.TableHeader, typeof(TableHeader) },
                    { ElementNames.TableCell, typeof(TableCell) },

                    { ElementNames.Poem, typeof(Poem) },
                    { ElementNames.Title, typeof(Title) },
                    { ElementNames.SubTitle, typeof(SubTitle) },
                    { ElementNames.EmptyLine, typeof(EmptyLine) },
                    { ElementNames.Epigraph, typeof(Epigraph) },
                    { ElementNames.Quote, typeof(Quote) },
                    { ElementNames.TextAuthor, typeof(TextAuthor) },
                    { ElementNames.Stanza, typeof(Stanza) },
                    { ElementNames.StanzaV, typeof(StanzaVerse) },

                    { ElementNames.Keywords, typeof(Keywords) },
                    { ElementNames.Coverpage, typeof(Coverpage) },
                    { ElementNames.Translator, typeof(Translator) },
                    { ElementNames.Sequence, typeof(SequenceInfo) },

                    { ElementNames.ProgramUsed, typeof(ProgramUsed) },
                    { ElementNames.SrcUrl, typeof(SrcUrl) },
                    { ElementNames.SrcOcr, typeof(SrcOcr) },
                    { ElementNames.Version, typeof(Models.Version) },
                    { ElementNames.History, typeof(History) },
                    { ElementNames.Publisher, typeof(Publisher) },

                    { ElementNames.BookName, typeof(BookName) },
                    { ElementNames.City, typeof(City) },
                    { ElementNames.Year, typeof(Year) },
                    { ElementNames.ISBN, typeof(ISBNInfo) },

                    { ElementNames.BookBody, typeof(BookBody) },
                    { ElementNames.BookBodySection, typeof(BodySection) },
                    { ElementNames.Stylesheet, typeof(Stylesheet) }
                }
            );

        private Fb2ElementFactory() { }

        public static Fb2ElementFactory Instance
        {
            get
            {
                if (instance == null)
                    instance = new Fb2ElementFactory();
                return instance;
            }
        }

        public Fb2Node GetElementByNodeName([In] string nodeName)
        {
            Type result = null;
            Type resultLowerInv = null;

            if (!KnownNodes.TryGetValue(nodeName, out result) &&
                !KnownNodes.TryGetValue(nodeName.ToLowerInvariant(), out resultLowerInv))
                return null;

            var modelType = result ?? resultLowerInv;

            if (modelType == null)
                return null;

            var model = Activator.CreateInstance(modelType) as Fb2Node;

            return model;
        }
    }
}
