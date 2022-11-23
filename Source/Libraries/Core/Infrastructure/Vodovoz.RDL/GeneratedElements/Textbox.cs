using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class Textbox
	{
		private List<object> itemsField = new List<object>();
		private List<ItemsChoiceType14> itemsElementNameField = new List<ItemsChoiceType14>();
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
		[XmlElement("CanGrow", typeof(bool))]
		[XmlElement("CanShrink", typeof(bool))]
		[XmlElement("CustomProperties", typeof(CustomProperties))]
		[XmlElement("DataElementName", typeof(string))]
		[XmlElement("DataElementOutput", typeof(TextboxTypeDataElementOutput))]
		[XmlElement("DataElementStyle", typeof(TextboxTypeDataElementStyle))]
		[XmlElement("Height", typeof(string), DataType = "normalizedString")]
		[XmlElement("HideDuplicates", typeof(string))]
		[XmlElement("Label", typeof(string))]
		[XmlElement("Left", typeof(string), DataType = "normalizedString")]
		[XmlElement("LinkToChild", typeof(string))]
		[XmlElement("RepeatWith", typeof(string))]
		[XmlElement("Style", typeof(Style))]
		[XmlElement("ToggleImage", typeof(ToggleImage))]
		[XmlElement("ToolTip", typeof(string))]
		[XmlElement("Top", typeof(string), DataType = "normalizedString")]
		[XmlElement("UserSort", typeof(UserSort))]
		[XmlElement("Value", typeof(string))]
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
		public override List<ItemsChoiceType14> ItemsElementNameList
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType14[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType14>() : value.ToList();
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
	public enum TextboxTypeDataElementOutput
	{
		Output,
		NoOutput,
		ContentsOnly,
		Auto,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum TextboxTypeDataElementStyle
	{
		Auto,
		AttributeNormal,
		ElementNormal,
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType14
	{
		[XmlEnum("##any:")]
		Item,
		Action,
		Bookmark,
		CanGrow,
		CanShrink,
		CustomProperties,
		DataElementName,
		DataElementOutput,
		DataElementStyle,
		Height,
		HideDuplicates,
		Label,
		Left,
		LinkToChild,
		RepeatWith,
		Style,
		ToggleImage,
		ToolTip,
		Top,
		UserSort,
		Value,
		Visibility,
		Width,
		ZIndex,
	}
}
