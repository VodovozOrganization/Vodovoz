using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Textbox
	{
		private object[] itemsField;
		private ItemsChoiceType14[] itemsElementNameField;
		private string nameField;
		private XmlAttribute[] anyAttrField;

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
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType14[] ItemsElementName
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