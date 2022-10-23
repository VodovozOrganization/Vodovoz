using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class DataCell
	{
		private DataValue[] dataValueField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("DataValue")]
		public DataValue[] DataValue
		{
			get => dataValueField;
			set => dataValueField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}