using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class SeriesGroupings
	{
		private List<SeriesGrouping> seriesGroupingField = new List<SeriesGrouping>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("SeriesGrouping")]
		public List<SeriesGrouping> SeriesGrouping
		{
			get => seriesGroupingField;
			set => seriesGroupingField = value;
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
