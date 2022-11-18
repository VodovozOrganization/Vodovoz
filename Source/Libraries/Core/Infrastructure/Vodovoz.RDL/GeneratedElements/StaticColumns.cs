using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class StaticColumns
	{
		private List<StaticColumn> staticColumnField = new List<StaticColumn>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("StaticColumn")]
		public List<StaticColumn> StaticColumn
		{
			get => staticColumnField;
			set => staticColumnField = value;
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
