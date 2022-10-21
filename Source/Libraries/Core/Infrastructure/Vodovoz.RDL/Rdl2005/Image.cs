using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Image
	{
		private object[] itemsField;
		private ItemsChoiceType15[] itemsElementNameField;
		private string nameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("Action", typeof(Action))]
		[XmlElement("Bookmark", typeof(string))]
		[XmlElement("CustomProperties", typeof(CustomProperties))]
		[XmlElement("DataElementName", typeof(string))]
		[XmlElement("DataElementOutput", typeof(ImageTypeDataElementOutput))]
		[XmlElement("Height", typeof(string), DataType = "normalizedString")]
		[XmlElement("Label", typeof(string))]
		[XmlElement("Left", typeof(string), DataType = "normalizedString")]
		[XmlElement("LinkToChild", typeof(string))]
		[XmlElement("MIMEType", typeof(string))]
		[XmlElement("RepeatWith", typeof(string))]
		[XmlElement("Sizing", typeof(ImageTypeSizing))]
		[XmlElement("Source", typeof(ImageTypeSource))]
		[XmlElement("Style", typeof(Style))]
		[XmlElement("ToolTip", typeof(string))]
		[XmlElement("Top", typeof(string), DataType = "normalizedString")]
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
		public ItemsChoiceType15[] ItemsElementName
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
	public enum ImageTypeDataElementOutput
	{
		Output,
		NoOutput,
		ContentsOnly,
		Auto,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ImageTypeSizing
	{
		AutoSize,
		Fit,
		FitProportional,
		Clip,
	}

	[Serializable()]
	[XmlType(AnonymousType = true)]
	public enum ImageTypeSource
	{
		External,
		Embedded,
		Database,
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType15
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
		MIMEType,
		RepeatWith,
		Sizing,
		Source,
		Style,
		ToolTip,
		Top,
		Value,
		Visibility,
		Width,
		ZIndex,
	}
}