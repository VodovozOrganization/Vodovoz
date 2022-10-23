using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class RowGroupings
	{
		private RowGrouping[] rowGroupingField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("RowGrouping")]
		public RowGrouping[] RowGrouping
		{
			get => rowGroupingField;
			set => rowGroupingField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}