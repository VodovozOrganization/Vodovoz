using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class CustomData
	{
		private object[] itemsField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("DataColumnGroupings", typeof(DataColumnGroupings))]
		[XmlElement("DataRowGroupings", typeof(DataRowGroupings))]
		[XmlElement("DataRows", typeof(DataRows))]
		[XmlElement("DataSetName", typeof(string))]
		[XmlElement("Filters", typeof(Filters))]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}