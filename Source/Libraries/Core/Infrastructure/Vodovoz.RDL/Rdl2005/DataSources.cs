using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class DataSources
	{
		private DataSource[] dataSourceField;
		private XmlAttribute[] anyAttrField;

		[XmlElement("DataSource")]
		public DataSource[] DataSource
		{
			get => dataSourceField;
			set => dataSourceField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}