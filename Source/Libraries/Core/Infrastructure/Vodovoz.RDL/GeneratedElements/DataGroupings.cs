using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class DataGroupings
	{
		private List<DataGrouping> dataGroupingField = new List<DataGrouping>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("DataGrouping")]
		public List<DataGrouping> DataGrouping
		{
			get => dataGroupingField;
			set => dataGroupingField = value;
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
