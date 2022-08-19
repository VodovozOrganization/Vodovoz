using System.ComponentModel;

namespace Vodovoz.ViewModels.Dialogs.Goods
{
	public interface INomenclatureGroupPricingItemViewModel : INotifyPropertyChanged
	{
		bool IsProductGroup { get; }
		string Name { get; }
		bool InvalidCostPurchasePrice { get; }
		decimal CostPurchasePrice { get; set; }
		bool InvalidInnerDeliveryPrice { get; }
		decimal InnerDeliveryPrice { get; set; }
	}
}
