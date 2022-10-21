using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class RowGrouping
	{
		private object[] itemsField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("DynamicRows", typeof(DynamicColumnsRows))]
		[XmlElement("FixedHeader", typeof(bool))]
		[XmlElement("StaticRows", typeof(StaticRows))]
		[XmlElement("Width", typeof(string), DataType = "normalizedString")]
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