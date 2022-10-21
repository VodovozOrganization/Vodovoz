using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class DataRows
	{
		private DataRow[] dataRowField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("DataRow")]
		public DataRow[] DataRow
		{
			get => dataRowField;
			set => dataRowField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}