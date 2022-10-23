using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class DataPoints
	{
		private DataPoint[] dataPointField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("DataPoint")]
		public DataPoint[] DataPoint
		{
			get => dataPointField;
			set => dataPointField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}