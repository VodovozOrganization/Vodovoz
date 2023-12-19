using System.Xml.Serialization;
using Vodovoz.RDL.Elements.Base;

namespace Vodovoz.RDL.Elements
{
	public partial class Textbox : BaseElementWithEnumedItems<ItemsChoiceType14>
	{
		[XmlIgnore]
		public string Action
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string Bookmark
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public bool CanGrow
		{
			get => GetEnamedItemsValue<bool>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public bool CanShrink
		{
			get => GetEnamedItemsValue<bool>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public CustomProperties CustomProperties
		{
			get => GetEnamedItemsValue<CustomProperties>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string DataElementName
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public TextboxTypeDataElementOutput DataElementOutput
		{
			get => GetEnamedItemsValue<TextboxTypeDataElementOutput>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public TextboxTypeDataElementStyle DataElementStyle
		{
			get => GetEnamedItemsValue<TextboxTypeDataElementStyle>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string HideDuplicates
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string Label
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string LinkToChild
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string RepeatWith
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public Style Style
		{
			get => GetEnamedItemsValue<Style>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public ToggleImage ToggleImage
		{
			get => GetEnamedItemsValue<ToggleImage>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string ToolTip
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public UserSort UserSort
		{
			get => GetEnamedItemsValue<UserSort>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string Value
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public Visibility Visibility
		{
			get => GetEnamedItemsValue<Visibility>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public uint ZIndex
		{
			get => GetEnamedItemsValue<uint>();
			set => SetEnamedItemsValue(value);
		}
	}
}
