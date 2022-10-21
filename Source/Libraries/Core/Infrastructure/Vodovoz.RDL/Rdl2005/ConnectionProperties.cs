using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class ConnectionProperties
	{
		private object[] itemsField;
		private ItemsChoiceType[] itemsElementNameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("ConnectString", typeof(string))]
		[XmlElement("DataProvider", typeof(string))]
		[XmlElement("IntegratedSecurity", typeof(bool))]
		[XmlElement("Prompt", typeof(string))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType[] ItemsElementName
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType
	{
		[XmlEnum("##any:")]
		Item,
		ConnectString,
		DataProvider,
		IntegratedSecurity,
		Prompt,
	}
}