using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class ColumnGrouping
	{
		private object[] itemsField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("DynamicColumns", typeof(DynamicColumnsRows))]
		[XmlElement("FixedHeader", typeof(bool))]
		[XmlElement("Height", typeof(string), DataType = "normalizedString")]
		[XmlElement("StaticColumns", typeof(StaticColumns))]
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