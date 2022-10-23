using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class TableCells
	{
		private TableCell[] tableCellField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("TableCell")]
		public TableCell[] TableCell
		{
			get => tableCellField;
			set => tableCellField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}