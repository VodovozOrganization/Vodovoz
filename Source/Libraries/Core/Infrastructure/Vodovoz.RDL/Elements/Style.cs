using System.Xml.Serialization;
using Vodovoz.RDL.Elements.Base;

namespace Vodovoz.RDL.Elements
{
	public partial class Style : BaseElementWithEnumedItems<ItemsChoiceType5>
	{
		[XmlIgnore]
		public string BackgroundColor
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string BackgroundGradientEndColor
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string BackgroundGradientType
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public BackgroundImage BackgroundImage
		{
			get => GetEnamedItemsValue<BackgroundImage>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public BorderColorStyleWidth BorderColor
		{
			get => GetEnamedItemsValue<BorderColorStyleWidth>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public BorderColorStyleWidth BorderStyle
		{
			get => GetEnamedItemsValue<BorderColorStyleWidth>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public BorderColorStyleWidth BorderWidth
		{
			get => GetEnamedItemsValue<BorderColorStyleWidth>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string Calendar
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string Color
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string Direction
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string FontFamily
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string FontSize
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string FontStyle
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string FontWeight
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string Format
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string Language
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string LineHeight
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string NumeralLanguage
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string NumeralVariant
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string PaddingBottom
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string PaddingLeft
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string PaddingRight
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string PaddingTop
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string TextAlign
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string TextDecoration
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string UnicodeBiDi
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string VerticalAlign
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string WritingMode
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}
	}
}
