using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fb2.Document.Constants;

namespace Fb2.Document.Playground;

public class Program
{
    public async static Task Main(string[] args)
    {
        var fb2Document = new Fb2Document();

        var x = Assembly.GetExecutingAssembly();
        var names = x.GetManifestResourceNames();

        // only one resource
        using (var fb2FileContentStream = x.GetManifestResourceStream(names[0]))
            await fb2Document.LoadAsync(fb2FileContentStream);

        var documentString = fb2Document.ToString();
        var xmlString = fb2Document.ToXmlString();

        var binaryImages = fb2Document.BinaryImages;
        var firstBodyTitle = fb2Document.Bodies.FirstOrDefault()?.GetFirstChild(ElementNames.Title);
    }
}