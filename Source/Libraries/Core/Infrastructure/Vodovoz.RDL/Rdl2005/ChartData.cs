using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class ChartData
	{
		private ChartSeries[] chartSeriesField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("ChartSeries")]
		public ChartSeries[] ChartSeries
		{
			get => chartSeriesField;
			set => chartSeriesField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}