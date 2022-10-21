using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class TableRows
	{
		private TableRow[] tableRowField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("TableRow")]
		public TableRow[] TableRow
		{
			get => tableRowField;
			set => tableRowField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}