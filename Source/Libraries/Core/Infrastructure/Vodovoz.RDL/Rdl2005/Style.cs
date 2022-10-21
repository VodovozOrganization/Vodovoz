using System;
using System.Xml.Serialization;
using System.Xml;

namespace Vodovoz.RDL.Rdl2005
{
	[Serializable()]
	[XmlType()]
	public partial class Style
	{
		private object[] itemsField;
		private ItemsChoiceType5[] itemsElementNameField;
		private XmlAttribute[] anyAttrField;

		[XmlAnyElement()]
		[XmlElement("BackgroundColor", typeof(string))]
		[XmlElement("BackgroundGradientEndColor", typeof(string))]
		[XmlElement("BackgroundGradientType", typeof(string))]
		[XmlElement("BackgroundImage", typeof(BackgroundImage))]
		[XmlElement("BorderColor", typeof(BorderColorStyleWidth))]
		[XmlElement("BorderStyle", typeof(BorderColorStyleWidth))]
		[XmlElement("BorderWidth", typeof(BorderColorStyleWidth))]
		[XmlElement("Calendar", typeof(string))]
		[XmlElement("Color", typeof(string))]
		[XmlElement("Direction", typeof(string))]
		[XmlElement("FontFamily", typeof(string))]
		[XmlElement("FontSize", typeof(string))]
		[XmlElement("FontStyle", typeof(string))]
		[XmlElement("FontWeight", typeof(string))]
		[XmlElement("Format", typeof(string))]
		[XmlElement("Language", typeof(string))]
		[XmlElement("LineHeight", typeof(string))]
		[XmlElement("NumeralLanguage", typeof(string))]
		[XmlElement("NumeralVariant", typeof(string))]
		[XmlElement("PaddingBottom", typeof(string))]
		[XmlElement("PaddingLeft", typeof(string))]
		[XmlElement("PaddingRight", typeof(string))]
		[XmlElement("PaddingTop", typeof(string))]
		[XmlElement("TextAlign", typeof(string))]
		[XmlElement("TextDecoration", typeof(string))]
		[XmlElement("UnicodeBiDi", typeof(string))]
		[XmlElement("VerticalAlign", typeof(string))]
		[XmlElement("WritingMode", typeof(string))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get => itemsField;
			set => itemsField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType5[] ItemsElementName
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlAnyAttribute()]
		public XmlAttribute[] AnyAttr
		{
			get => anyAttrField;
			set => anyAttrField = value;
		}
	}

	[Serializable()]
	[XmlType(IncludeInSchema = false)]
	public enum ItemsChoiceType5
	{
		[XmlEnum("##any:")]
		Item,
		BackgroundColor,
		BackgroundGradientEndColor,
		BackgroundGradientType,
		BackgroundImage,
		BorderColor,
		BorderStyle,
		BorderWidth,
		Calendar,
		Color,
		Direction,
		FontFamily,
		FontSize,
		FontStyle,
		FontWeight,
		Format,
		Language,
		LineHeight,
		NumeralLanguage,
		NumeralVariant,
		PaddingBottom,
		PaddingLeft,
		PaddingRight,
		PaddingTop,
		TextAlign,
		TextDecoration,
		UnicodeBiDi,
		VerticalAlign,
		WritingMode,
	}
}