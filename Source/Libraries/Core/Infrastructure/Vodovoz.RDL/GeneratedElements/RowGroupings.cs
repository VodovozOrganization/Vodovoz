using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class RowGroupings
	{
		private List<RowGrouping> rowGroupingField = new List<RowGrouping>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("RowGrouping")]
		public List<RowGrouping> RowGrouping
		{
			get => rowGroupingField;
			set => rowGroupingField = value;
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
