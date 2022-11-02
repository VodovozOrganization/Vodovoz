using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class Rectangle
	{
		private List<object> itemsField = new List<object>();
		private List<ItemsChoiceType12> itemsElementNameField = new List<ItemsChoiceType12>();
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
		[XmlElement("DataElementOutput", typeof(RectangleTypeDataElementOutput))]
		[XmlElement("Height", typeof(string), DataType = "normalizedString")]
		[XmlElement("Label", typeof(string))]
		[XmlElement("Left", typeof(string), DataType = "normalizedString")]
		[XmlElement("LinkToChild", typeof(string))]
		[XmlElement("PageBreakAtEnd", typeof(bool))]
		[XmlElement("PageBreakAtStart", typeof(bool))]
		[XmlElement("RepeatWith", typeof(string))]
		[XmlElement("ReportItems", typeof(ReportItems))]
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
		public List<ItemsChoiceType12> ItemsElementNameList
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType12[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType12>() : value.ToList();
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
	public enum RectangleTypeDataElementOutput
	{
		Output,
		NoOutput,
		ContentsOnly,
		Auto,
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType12
	{
		[XmlEnum("##any:")]
		Item,
		Action,
		Bookmark,
		CustomProperties,
		DataElementName,
		DataElementOutput,
		Height,
		Label,
		Left,
		LinkToChild,
		PageBreakAtEnd,
		PageBreakAtStart,
		RepeatWith,
		ReportItems,
		Style,
		ToolTip,
		Top,
		Visibility,
		Width,
		ZIndex,
	}
}
