using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class ColumnGroupings
	{
		private ColumnGrouping[] columnGroupingField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("ColumnGrouping")]
		public ColumnGrouping[] ColumnGrouping
		{
			get => columnGroupingField;
			set => columnGroupingField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}