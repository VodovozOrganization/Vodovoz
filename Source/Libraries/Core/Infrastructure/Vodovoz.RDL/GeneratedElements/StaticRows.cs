using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class StaticRows
	{
		private List<StaticRow> staticRowField = new List<StaticRow>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("StaticRow")]
		public List<StaticRow> StaticRow
		{
			get => staticRowField;
			set => staticRowField = value;
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
