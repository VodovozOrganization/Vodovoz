using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class Matrix
	{
		private List<object> itemsField = new List<object>();
		private List<ItemsChoiceType19> itemsElementNameField = new List<ItemsChoiceType19>();
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
		[XmlElement("CellDataElementName", typeof(string))]
		[XmlElement("CellDataElementOutput", typeof(MatrixTypeCellDataElementOutput))]
		[XmlElement("ColumnGroupings", typeof(ColumnGroupings))]
		[XmlElement("Corner", typeof(Corner))]
		[XmlElement("CustomProperties", typeof(CustomProperties))]
		[XmlElement("DataElementName", typeof(string))]
		[XmlElement("DataElementOutput", typeof(MatrixTypeDataElementOutput))]
		[XmlElement("DataSetName", typeof(string))]
		[XmlElement("Filters", typeof(Filters))]
		[XmlElement("GroupsBeforeRowHeaders", typeof(uint))]
		[XmlElement("Height", typeof(string), DataType = "normalizedString")]
		[XmlElement("KeepTogether", typeof(bool))]
		[XmlElement("Label", typeof(string))]
		[XmlElement("LayoutDirection", typeof(MatrixTypeLayoutDirection))]
		[XmlElement("Left", typeof(string), DataType = "normalizedString")]
		[XmlElement("LinkToChild", typeof(string))]
		[XmlElement("MatrixColumns", typeof(MatrixColumns))]
		[XmlElement("MatrixRows", typeof(MatrixRows))]
		[XmlElement("NoRows", typeof(string))]
		[XmlElement("PageBreakAtEnd", typeof(bool))]
		[XmlElement("PageBreakAtStart", typeof(bool))]
		[XmlElement("RepeatWith", typeof(string))]
		[XmlElement("RowGroupings", typeof(RowGroupings))]
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
		public List<ItemsChoiceType19> ItemsElementNameList
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType19[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType19>() : value.ToList();
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
	public enum MatrixTypeCellDataElementOutput
	{
		Output,
		NoOutput,
		ContentsOnly,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum MatrixTypeDataElementOutput
	{
		Output,
		NoOutput,
		ContentsOnly,
		Auto,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum MatrixTypeLayoutDirection
	{
		LTR,
		RTL,
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType19
	{
		[XmlEnum("##any:")]
		Item,
		Action,
		Bookmark,
		CellDataElementName,
		CellDataElementOutput,
		ColumnGroupings,
		Corner,
		CustomProperties,
		DataElementName,
		DataElementOutput,
		DataSetName,
		Filters,
		GroupsBeforeRowHeaders,
		Height,
		KeepTogether,
		Label,
		LayoutDirection,
		Left,
		LinkToChild,
		MatrixColumns,
		MatrixRows,
		NoRows,
		PageBreakAtEnd,
		PageBreakAtStart,
		RepeatWith,
		RowGroupings,
		Style,
		ToolTip,
		Top,
		Visibility,
		Width,
		ZIndex,
	}
}
