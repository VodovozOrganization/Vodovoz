using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Marker
	{
		private object[] itemsField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("Size", typeof(string), DataType = "normalizedString")]
		[XmlElement("Style", typeof(Style))]
		[XmlElement("Type", typeof(MarkerType))]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum MarkerType
	{
		None,
		Square,
		Circle,
		Diamond,
		Triangle,
		Cross,
		Auto,
	}
}