using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class ColumnGroupings
	{
		private List<ColumnGrouping> columnGroupingField = new List<ColumnGrouping>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("ColumnGrouping")]
		public List<ColumnGrouping> ColumnGrouping
		{
			get => columnGroupingField;
			set => columnGroupingField = value;
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
