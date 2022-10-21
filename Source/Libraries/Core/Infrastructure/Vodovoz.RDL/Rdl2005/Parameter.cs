using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Parameter
	{
		private object[] itemsField;
		private ItemsChoiceType6[] itemsElementNameField;
		private string nameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("Omit", typeof(string))]
		[XmlElement("Value", typeof(string))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType6[] ItemsElementName
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
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

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType6
	{
		[XmlEnum("##any:")]
		Item,
		Omit,
		Value,
	}
}