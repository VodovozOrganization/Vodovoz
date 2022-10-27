using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class Table
	{
		private List<object> itemsField = new List<object>();
		private List<ItemsChoiceType21> itemsElementNameField = new List<ItemsChoiceType21>();
		private string nameField;
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlIgnore()]
		public override List<object> ItemsList
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlAnyElement()]
		[XmlElement("Action", typeof(Action))]
		[XmlElement("Bookmark", typeof(string))]
		[XmlElement("CustomProperties", typeof(CustomProperties))]
		[XmlElement("DataElementName", typeof(string))]
		[XmlElement("DataElementOutput", typeof(TableTypeDataElementOutput))]
		[XmlElement("DataSetName", typeof(string))]
		[XmlElement("DetailDataCollectionName", typeof(string))]
		[XmlElement("DetailDataElementName", typeof(string))]
		[XmlElement("DetailDataElementOutput", typeof(TableTypeDetailDataElementOutput))]
		[XmlElement("Details", typeof(Details))]
		[XmlElement("FillPage", typeof(bool))]
		[XmlElement("Filters", typeof(Filters))]
		[XmlElement("Footer", typeof(Footer))]
		[XmlElement("Header", typeof(Header))]
		[XmlElement("Height", typeof(string), DataType = "normalizedString")]
		[XmlElement("KeepTogether", typeof(bool))]
		[XmlElement("Label", typeof(string))]
		[XmlElement("Left", typeof(string), DataType = "normalizedString")]
		[XmlElement("LinkToChild", typeof(string))]
		[XmlElement("NoRows", typeof(string))]
		[XmlElement("PageBreakAtEnd", typeof(bool))]
		[XmlElement("PageBreakAtStart", typeof(bool))]
		[XmlElement("RepeatWith", typeof(string))]
		[XmlElement("Style", typeof(Style))]
		[XmlElement("TableColumns", typeof(TableColumns))]
		[XmlElement("TableGroups", typeof(TableGroups))]
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
		public override List<ItemsChoiceType21> ItemsElementNameList
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType21[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType21>() : value.ToList();
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
	public enum TableTypeDataElementOutput
	{
		Output,
		NoOutput,
		ContentsOnly,
		Auto,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum TableTypeDetailDataElementOutput
	{
		Output,
		NoOutput,
		ContentsOnly,
	}

	[Serializable()]
	[XmlType()]
	public enum ItemsChoiceType21
	{
		[XmlEnum("##any:")]
		Item,
		Action,
		Bookmark,
		CustomProperties,
		DataElementName,
		DataElementOutput,
		DataSetName,
		DetailDataCollectionName,
		DetailDataElementName,
		DetailDataElementOutput,
		Details,
		FillPage,
		Filters,
		Footer,
		Header,
		Height,
		KeepTogether,
		Label,
		Left,
		LinkToChild,
		NoRows,
		PageBreakAtEnd,
		PageBreakAtStart,
		RepeatWith,
		Style,
		TableColumns,
		TableGroups,
		ToolTip,
		Top,
		Visibility,
		Width,
		ZIndex,
	}
}
