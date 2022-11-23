using System.ComponentModel;

namespace Vodovoz.ViewModels.Dialogs.Goods
{
	public interface INomenclatureGroupPricingItemViewModel : INotifyPropertyChanged
	{
		bool IsGroup { get; }
		string Name { get; }
		bool InvalidCostPrice { get; }
		decimal CostPrice { get; set; }
		bool InvalidInnerDeliveryPrice { get; }
		decimal InnerDeliveryPrice { get; set; }
	}
}
