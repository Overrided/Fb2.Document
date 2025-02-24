using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fb2.Document.Constants;
using Fb2.Document.LoadingOptions;

namespace Fb2.Document.Playground;

public class Program
{
    public async static Task Main(string[] args)
    {
        var fb2Document = new Fb2Document();

        var fb2Document1 = new Fb2Document();

        var x = Assembly.GetExecutingAssembly();
        var names = x.GetManifestResourceNames();

        // only one resource
        using (var fb2FileContentStream = x.GetManifestResourceStream(names[0]))
        {
            await fb2Document.LoadAsync(fb2FileContentStream, new Fb2StreamLoadingOptions { CloseInputStream = false });

            fb2FileContentStream.Seek(0, System.IO.SeekOrigin.Begin);

            await fb2Document1.LoadOptimizedAsync(fb2FileContentStream);
            var a = 1;
        }
        //await fb2Document.LoadAsync(fb2FileContentStream);

        //var documentString = fb2Document.ToString();
        //var xmlString = fb2Document.ToXmlString();

        //var binaryImages = fb2Document.BinaryImages;
        //var firstBodyTitle = fb2Document.Bodies.FirstOrDefault()?.GetFirstChild(ElementNames.Title);
    }
}