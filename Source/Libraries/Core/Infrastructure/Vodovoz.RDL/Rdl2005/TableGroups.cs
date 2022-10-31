using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType(IncludeInSchema = true)]
	public partial class TableGroups
	{
		private TableGroup[] tableGroupField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("TableGroup")]
		public TableGroup[] TableGroup
		{
			get => tableGroupField;
			set => tableGroupField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}