using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class List
	{
		private List<object> itemsField = new List<object>();
		private List<ItemsChoiceType18> itemsElementNameField = new List<ItemsChoiceType18>();
		private string nameField;
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlIgnore()]
		public List<object> ItemsList
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlAnyElement()]
		[XmlElement("Action", typeof(Action))]
		[XmlElement("Bookmark", typeof(string))]
		[XmlElement("CustomProperties", typeof(CustomProperties))]
		[XmlElement("DataElementName", typeof(string))]
		[XmlElement("DataElementOutput", typeof(ListTypeDataElementOutput))]
		[XmlElement("DataInstanceElementOutput", typeof(ListTypeDataInstanceElementOutput))]
		[XmlElement("DataInstanceName", typeof(string))]
		[XmlElement("DataSetName", typeof(string))]
		[XmlElement("FillPage", typeof(bool))]
		[XmlElement("Filters", typeof(Filters))]
		[XmlElement("Grouping", typeof(Grouping))]
		[XmlElement("Height", typeof(string), DataType = "normalizedString")]
		[XmlElement("KeepTogether", typeof(bool))]
		[XmlElement("Label", typeof(string))]
		[XmlElement("Left", typeof(string), DataType = "normalizedString")]
		[XmlElement("LinkToChild", typeof(string))]
		[XmlElement("NoRows", typeof(string))]
		[XmlElement("PageBreakAtEnd", typeof(bool))]
		[XmlElement("PageBreakAtStart", typeof(bool))]
		[XmlElement("RepeatWith", typeof(string))]
		[XmlElement("ReportItems", typeof(ReportItems))]
		[XmlElement("Sorting", typeof(Sorting))]
		[XmlElement("Style", typeof(Style))]
		[XmlElement("ToolTip", typeof(string))]
		[XmlElement("Top", typeof(string), DataType = "normalizedString")]
		[XmlElement("Visibility", typeof(Visibility))]
		[XmlElement("Width", typeof(string), DataType = "normalizedString")]
		[XmlElement("ZIndex", typeof(uint))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => ItemsList.ToArray();
			set => ItemsList = value == null ? new List<object>() : value.ToList();
		}

		[XmlIgnore()]
		public List<ItemsChoiceType18> ItemsElementNameList
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType18[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType18>() : value.ToList();
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
	[XmlType(AnonymousType = true)]
	public enum ListTypeDataElementOutput
	{
		Output,
		NoOutput,
		ContentsOnly,
		Auto,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ListTypeDataInstanceElementOutput
	{
		Output,
		NoOutput,
		ContentsOnly,
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType18
	{
		[XmlEnum("##any:")]
		Item,
		Action,
		Bookmark,
		CustomProperties,
		DataElementName,
		DataElementOutput,
		DataInstanceElementOutput,
		DataInstanceName,
		DataSetName,
		FillPage,
		Filters,
		Grouping,
		Height,
		KeepTogether,
		Label,
		Left,
		LinkToChild,
		NoRows,
		PageBreakAtEnd,
		PageBreakAtStart,
		RepeatWith,
		ReportItems,
		Sorting,
		Style,
		ToolTip,
		Top,
		Visibility,
		Width,
		ZIndex,
	}
}
