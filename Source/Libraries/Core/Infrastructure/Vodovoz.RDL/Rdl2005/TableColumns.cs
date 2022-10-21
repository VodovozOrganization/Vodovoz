using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class TableColumns
	{
		private TableColumn[] tableColumnField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("TableColumn")]
		public TableColumn[] TableColumn
		{
			get => tableColumnField;
			set => tableColumnField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}