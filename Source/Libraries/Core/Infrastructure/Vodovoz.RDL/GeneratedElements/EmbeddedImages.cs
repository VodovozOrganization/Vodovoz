using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class EmbeddedImages
	{
		private List<EmbeddedImage> embeddedImageField = new List<EmbeddedImage>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("EmbeddedImage")]
		public List<EmbeddedImage> EmbeddedImage
		{
			get => embeddedImageField;
			set => embeddedImageField = value;
		}

		[XmlIgnore()]
		public List<XmlAttribute> AnyAttrList
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => AnyAttrList.ToArray();
			set => AnyAttrList = value == null ? new List<XmlAttribute>() : value.ToList();
		}
	}
}
