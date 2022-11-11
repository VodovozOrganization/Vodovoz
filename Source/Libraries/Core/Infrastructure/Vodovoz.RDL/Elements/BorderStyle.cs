using System.Xml.Serialization;
using Vodovoz.RDL.Elements.Base;

namespace Vodovoz.RDL.Elements
{
	public partial class BorderColorStyleWidth : BaseElementWithEnumedItems<ItemsChoiceType3>
	{
		[XmlIgnore]
		public string Bottom
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string Default
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string Left
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string Right
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

		[XmlIgnore]
		public string Top
		{
			get => GetEnamedItemsValue<string>();
			set => SetEnamedItemsValue(value);
		}

	}
}
