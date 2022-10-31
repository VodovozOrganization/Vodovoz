using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class DataRow
	{
		private DataCell[] dataCellField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("DataCell")]
		public DataCell[] DataCell
		{
			get => dataCellField;
			set => dataCellField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}