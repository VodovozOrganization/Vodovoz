using System.Xml.Serialization;
using Vodovoz.RDL.Elements.Base;

namespace Vodovoz.RDL.Elements
{
	public partial class TableGroup : BaseElementWithItems
	{
		[XmlIgnore]
		public Footer Footer
		{
			get => GetItemsValue<Footer>();
			set => SetItemsValue(value);
		}

		[XmlIgnore]
		public Grouping Grouping
		{
			get => GetItemsValue<Grouping>();
			set => SetItemsValue(value);
		}

		[XmlIgnore]
		public Header Header
		{
			get => GetItemsValue<Header>();
			set => SetItemsValue(value);
		}

		[XmlIgnore]
		public Sorting Sorting
		{
			get => GetItemsValue<Sorting>();
			set => SetItemsValue(value);
		}

		[XmlIgnore]
		public Visibility Visibility
		{
			get => GetItemsValue<Visibility>();
			set => SetItemsValue(value);
		}
	}
}
