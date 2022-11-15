using System;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.RDL.Elements
{
	[Serializable()]
	[XmlType()]
	public partial class Style
	{
		private List<object> itemsField = new List<object>();
		private List<ItemsChoiceType5> itemsElementNameField = new List<ItemsChoiceType5>();
		private List<XmlAttribute> anyAttrField = new List<XmlAttribute>();

		[XmlIgnore()]
		public override List<object> ItemsList
		{
			get => itemsField;
			set => itemsField = value;
		}

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
			get => ItemsList.ToArray();
			set => ItemsList = value == null ? new List<object>() : value.ToList();
		}

		[XmlIgnore()]
		public override List<ItemsChoiceType5> ItemsElementNameList
		{
			get => itemsElementNameField;
			set => itemsElementNameField = value;
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore()]
		public ItemsChoiceType5[] ItemsElementName
		{
			get => ItemsElementNameList.ToArray();
			set => ItemsElementNameList = value == null ? new List<ItemsChoiceType5>() : value.ToList();
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
