using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class List
	{
		private object[] itemsField;
		private ItemsChoiceType18[] itemsElementNameField;
		private string nameField;
		private XmlAttribute[] anyAttrField;

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
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType18[] ItemsElementName
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlAttribute(DataType = "normalizedString")]
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