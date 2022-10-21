using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class DataGroupings
	{
		private DataGrouping[] dataGroupingField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("DataGrouping")]
		public DataGrouping[] DataGrouping
		{
			get => dataGroupingField;
			set => dataGroupingField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}