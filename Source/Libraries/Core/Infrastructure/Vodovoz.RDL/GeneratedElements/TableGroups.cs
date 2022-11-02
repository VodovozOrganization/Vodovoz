using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType(IncludeInSchema = true)]
	public partial class TableGroups
	{
		private List<TableGroup> tableGroupField = new List<TableGroup>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlElement("TableGroup")]
		public List<TableGroup> TableGroup
		{
			get => tableGroupField;
			set => tableGroupField = value;
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
