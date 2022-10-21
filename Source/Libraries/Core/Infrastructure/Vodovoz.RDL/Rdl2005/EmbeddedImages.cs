using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class EmbeddedImages
	{
		private EmbeddedImage[] embeddedImageField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("EmbeddedImage")]
		public EmbeddedImage[] EmbeddedImage
		{
			get => embeddedImageField;
			set => embeddedImageField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}