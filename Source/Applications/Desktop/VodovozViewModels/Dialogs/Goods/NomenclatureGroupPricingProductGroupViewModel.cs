using QS.ViewModels;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.ViewModels.Dialogs.Goods
{
	public class NomenclatureGroupPricingProductGroupViewModel : ViewModelBase, INomenclatureGroupPricingItemViewModel
	{
		private readonly ProductGroup _productGroup;
		private List<NomenclatureGroupPricingItemViewModel> _priceViewModels { get; set; }

		public NomenclatureGroupPricingProductGroupViewModel(ProductGroup productGroup)
		{
			_priceViewModels = new List<NomenclatureGroupPricingItemViewModel>();
			_productGroup = productGroup ?? throw new ArgumentNullException(nameof(productGroup));
		}

		public bool IsGroup => true;

		public string Name => _productGroup.Name;

		bool INomenclatureGroupPricingItemViewModel.InvalidCostPrice => false;

		decimal INomenclatureGroupPricingItemViewModel.CostPrice { get; set; }

		bool INomenclatureGroupPricingItemViewModel.InvalidInnerDeliveryPrice => false;

		decimal INomenclatureGroupPricingItemViewModel.InnerDeliveryPrice { get; set; }

		public IList<NomenclatureGroupPricingItemViewModel> PriceViewModels => _priceViewModels;

		internal void AddPriceViewModel(NomenclatureGroupPricingItemViewModel priceViewModel)
		{
			_priceViewModels.Add(priceViewModel);
		}

		internal void ClearPriceViewModels()
		{
			_priceViewModels.Clear();
		}

	}
}
