using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fb2.Document.Constants;

namespace Fb2.Document.Playground;

public class Program
{
    public async static Task Main(string[] args)
    {
        var sampleFilePath = GetSampleFilePath();
        var fb2Document = new Fb2Document();

        using (var fb2FileContentStream = new FileStream(sampleFilePath, FileMode.Open))
        {
            await fb2Document.LoadAsync(fb2FileContentStream);
        }

        var documentString = fb2Document.ToString();
        var xmlString = fb2Document.ToXmlString();

        var binaryImages = fb2Document.BinaryImages;
        var firstBodyTitle = fb2Document.Bodies.FirstOrDefault()?.GetFirstChild(ElementNames.Title);
    }

    private static string GetSampleFilePath()
    {
        var currentFolder = Environment.CurrentDirectory;
        var pathChunks = currentFolder.Split(Path.DirectorySeparatorChar).ToList();
        var pathChunksTopIndex = pathChunks.Count - 1;
        for (int i = pathChunksTopIndex; i >= 0; i--)
        {
            var chunk = pathChunks[i];
            if (chunk.Equals("Fb2.Document"))
            {
                pathChunks.RemoveRange(i + 1, pathChunksTopIndex - i);
                break;
            }
        }

        pathChunks.Add("Sample");
        pathChunks.Add("_Test_1.fb2");

        var sampleFilePath = Path.Combine(pathChunks.ToArray());
        return sampleFilePath;
    }
}