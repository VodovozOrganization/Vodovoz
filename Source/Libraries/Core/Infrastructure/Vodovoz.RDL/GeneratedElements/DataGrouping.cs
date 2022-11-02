using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class DataGrouping
	{
		private List<object> itemsField = new List<object>();
		private List<ItemsChoiceType28> itemsElementNameField = new List<ItemsChoiceType28>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlIgnore()]
		public List<object> ItemsList
		{
			get => itemsField;
			set => itemsField = value;
		}

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
			get => ItemsList.ToArray();
			set => ItemsList = value == null ? new List<object>() : value.ToList();
		}

		[XmlIgnore()]
		public List<ItemsChoiceType28> ItemsElementNameList
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType28[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType28>() : value.ToList();
		}

		[XmlIgnore()]
		public List<XmlAttribute> AnyAttrList
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => AnyAttrList.ToArray();
			set => AnyAttrList = value == null ? new List<XmlAttribute>() : value.ToList();
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
