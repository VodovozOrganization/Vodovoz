using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Title
	{
		private object[] itemsField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("Caption", typeof(string))]
		[XmlElement("Position", typeof(TitleTypePosition))]
		[XmlElement("Style", typeof(Style))]
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
	public enum TitleTypePosition
	{
		Center,
		Near,
		Far,
	}
}