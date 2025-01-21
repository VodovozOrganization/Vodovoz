using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace TaxcomEdo.Contracts.XmlWrappers.Warrant
{
	[GeneratedCode("xsd", "4.6.1590.0")]
	[DesignerCategory("code")]
	[XmlRoot(ElementName = "WarrantCard", Namespace = "", IsNullable = false)]
	[Serializable]
	public class WarrantCard
	{
		private WarrantCardDescription descriptionField;
		private WarrantCardDocSign[] toSignField;
		private WarrantCardAdditionalParameter[] additionalDataField;
		private static XmlSchemaSet _metaXmlSchemaSet;

		public WarrantCardDescription Description
		{
			get => this.descriptionField;
			set => this.descriptionField = value;
		}

		[XmlArrayItem("DocSign", IsNullable = false)]
		public WarrantCardDocSign[] ToSign
		{
			get => this.toSignField;
			set => this.toSignField = value;
		}

		[XmlArrayItem("AdditionalParameter", IsNullable = false)]
		public WarrantCardAdditionalParameter[] AdditionalData
		{
			get => this.additionalDataField;
			set => this.additionalDataField = value;
		}

		/*public static Warrant CreateWarrant(byte[] warrantBody)
		{
		  using (MemoryStream memoryStream = new MemoryStream(warrantBody))
		    return (Warrant) new XmlSerializer(typeof (Warrant)).Deserialize((Stream) memoryStream);
		}

		public static Warrant CreateWarrant(XmlNode warrantNode)
		{
		  if (warrantNode == null)
		    throw new ArgumentNullException(nameof (warrantNode));
		  if (!Taxcom.TTC.Container.Container.IgnoreSchemaCheck)
		  {
		    XElement xelement1 = XElement.Parse(warrantNode.OuterXml);
		    foreach (XElement xelement2 in xelement1.DescendantsAndSelf())
		      xelement2.Name = XName.Get(xelement2.Name.LocalName, ContainerWarrantConstants.WarrantInfoXmlNamespace);
		    ValidationEventHandler validationEventHandler = (ValidationEventHandler) ((o, e) =>
		    {
		      if(e.Severity == XmlSeverityType.Error)
		        throw new BuildContainerCardXmlSchemaNotMatchException((string) null, (Exception) e.Exception);
		    });
		    new XDocument(new object[1]{ (object) xelement1 }).Validate(WarrantCard.WarrantXmlSchemaSet, validationEventHandler);
		  }
		  using(var xmlNodeReader = new XmlNodeReader(warrantNode))
		    return (WarrantXml)new XmlSerializer(typeof(WarrantXml)).Deserialize(xmlNodeReader);
		}

		internal static XmlSchemaSet WarrantXmlSchemaSet
		{
		  get
		  {
		    if (WarrantCard._metaXmlSchemaSet != null)
		      return WarrantCard._metaXmlSchemaSet;
		    XmlSchemaSet xmlSchemaSet = new XmlSchemaSet();
		    bool hasSchemaParseError = false;
		    using (XmlReader reader = (XmlReader) new XmlTextReader(Resources.warrant, XmlNodeType.Document, (XmlParserContext) null))
		      xmlSchemaSet.Add(XmlSchema.Read(reader, (ValidationEventHandler) ((s, e) => hasSchemaParseError |= e.Severity == XmlSeverityType.Error)));
		    xmlSchemaSet.Compile();
		    return WarrantCard._metaXmlSchemaSet = xmlSchemaSet;
		  }
		}

		public static byte[] ChangeTextInImage(byte[] image, string source, string target)
		{
		  if (image == null)
		    throw new ArgumentNullException(nameof (image));
		  source = "\"" + source + "\"";
		  target = "\"" + target + "\"";
		  if (string.Equals(source, target, StringComparison.InvariantCultureIgnoreCase))
		    return image;
		  string s = Encoding.GetEncoding(1251).GetString(image, 0, image.Length).Replace(source, target);
		  return Encoding.GetEncoding(1251).GetBytes(s);
		}
	  }*/
	}
}
