using System.Xml.Linq;

namespace Fb2.Document.SerializingOptions;

/// <summary>
/// Specifies a set of options to use with <see cref="Fb2Document.ToXml(Fb2XmlSerializingOptions?)"/> and <see cref="Fb2Document.ToXmlString(Fb2XmlSerializingOptions?)"/>.
/// </summary>
public class Fb2XmlSerializingOptions
{
    /// <summary>
    /// Indicates if Unsafe elements should be serialized.
    /// </summary>
    public bool SerializeUnsafeElements { get; set; }

    /// <summary>
    /// <see cref="System.Xml.Linq.XDeclaration"/> to use for <see cref="XDocument"/> creation during serialization.
    /// </summary>
    public XDeclaration? XDeclaration { get; set; }

    /// <summary>
    /// Creates new instance of <see cref="Fb2XmlSerializingOptions"/>.
    /// </summary>
    /// <param name="serializeUnsafeElements">Indicates if Unsafe elements should be serialized. Optional, <see langword="true"/> by default.</param>
    /// <param name="xDeclaration"> <see cref="System.Xml.Linq.XDeclaration"/> to use for <see cref="XDocument"/> creation during serialization. Optional, <see langword="null"/> by default. </param>
    public Fb2XmlSerializingOptions(bool serializeUnsafeElements = true, XDeclaration? xDeclaration = null)
    {
        SerializeUnsafeElements = serializeUnsafeElements;
        XDeclaration = xDeclaration;
    }
}
