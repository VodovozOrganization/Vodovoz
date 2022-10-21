using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class SeriesGroupings
	{
		private SeriesGrouping[] seriesGroupingField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("SeriesGrouping")]
		public SeriesGrouping[] SeriesGrouping
		{
			get => seriesGroupingField;
			set => seriesGroupingField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}