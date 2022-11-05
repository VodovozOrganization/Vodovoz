using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class ChartData
	{
		private List<ChartSeries> chartSeriesField = new List<ChartSeries>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("ChartSeries")]
		public List<ChartSeries> ChartSeries
		{
			get => chartSeriesField;
			set => chartSeriesField = value;
		}

		[XmlIgnore()]
		public List<XmlAttribute> AnyAttrList
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => AnyAttrList.ToArray();
			set => AnyAttrList = value == null ? new List<XmlAttribute>() : value.ToList();
		}
	}
}
