using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class Grouping
	{
		private List<object> itemsField = new List<object>();
		private List<ItemsChoiceType17> itemsElementNameField = new List<ItemsChoiceType17>();
		private string nameField;
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlIgnore()]
		public override List<object> ItemsList
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlAnyElement()]
		[XmlElement("CustomProperties", typeof(CustomProperties))]
		[XmlElement("DataCollectionName", typeof(string))]
		[XmlElement("DataElementName", typeof(string))]
		[XmlElement("DataElementOutput", typeof(GroupingTypeDataElementOutput))]
		[XmlElement("Filters", typeof(Filters))]
		[XmlElement("GroupExpressions", typeof(GroupExpressions))]
		[XmlElement("Label", typeof(string))]
		[XmlElement("PageBreakAtEnd", typeof(bool))]
		[XmlElement("PageBreakAtStart", typeof(bool))]
		[XmlElement("Parent", typeof(string))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => ItemsList.ToArray();
			set => ItemsList = value == null ? new List<object>() : value.ToList();
		}

		[XmlIgnore()]
		public override List<ItemsChoiceType17> ItemsElementNameList
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType17[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType17>() : value.ToList();
		}

		[XmlAttribute(DataType = "normalizedString")]
		public string Name
		{
			get => nameField;
			set => nameField = value;
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
	public enum ItemsChoiceType17
	{
		[XmlEnum("##any:")]
		Item,
		CustomProperties,
		DataCollectionName,
		DataElementName,
		DataElementOutput,
		Filters,
		GroupExpressions,
		Label,
		PageBreakAtEnd,
		PageBreakAtStart,
		Parent,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum GroupingTypeDataElementOutput
	{
		Output,
		NoOutput,
		ContentsOnly,
	}
}
