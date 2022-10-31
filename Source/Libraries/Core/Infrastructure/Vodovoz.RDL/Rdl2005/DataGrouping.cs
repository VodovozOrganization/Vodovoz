using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class DataGrouping
	{
		private object[] itemsField;
		private ItemsChoiceType28[] itemsElementNameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("CustomProperties", typeof(CustomProperties))]
		[XmlElement("DataGroupings", typeof(DataGroupings))]
		[XmlElement("Grouping", typeof(Grouping))]
		[XmlElement("Sorting", typeof(Sorting))]
		[XmlElement("Static", typeof(bool))]
		[XmlElement("Subtotal", typeof(bool))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType28[] ItemsElementName
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
	public enum ItemsChoiceType28
	{
		[XmlEnum("##any:")]
		Item,
		CustomProperties,
		DataGroupings,
		Grouping,
		Sorting,
		Static,
		Subtotal,
	}
}