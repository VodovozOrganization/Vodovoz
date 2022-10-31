using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class DataSource
	{
		private object[] itemsField;
		private string nameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("ConnectionProperties", typeof(ConnectionProperties))]
		[XmlElement("DataSourceReference", typeof(string))]
		[XmlElement("Transaction", typeof(bool))]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlAttribute()]
		public string Name
		{
			get => nameField;
			set => nameField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}
}