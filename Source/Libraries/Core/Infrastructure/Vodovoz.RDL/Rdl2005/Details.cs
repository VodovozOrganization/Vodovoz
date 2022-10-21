using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Details
	{
		private object[] itemsField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("Grouping", typeof(Grouping))]
		[XmlElement("Sorting", typeof(Sorting))]
		[XmlElement("TableRows", typeof(TableRows))]
		[XmlElement("Visibility", typeof(Visibility))]
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
}