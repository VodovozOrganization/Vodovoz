using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Action
	{
		private object[] itemsField;
		private ItemsChoiceType8[] itemsElementNameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("BookmarkLink", typeof(string))]
		[XmlElement("Drillthrough", typeof(Drillthrough))]
		[XmlElement("Hyperlink", typeof(string))]
		[XmlElement("Label", typeof(string))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType8[] ItemsElementName
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
	public enum ItemsChoiceType8
	{
		[XmlEnum("##any:")]
		Item,
		BookmarkLink,
		Drillthrough,
		Hyperlink,
		Label,
	}
}