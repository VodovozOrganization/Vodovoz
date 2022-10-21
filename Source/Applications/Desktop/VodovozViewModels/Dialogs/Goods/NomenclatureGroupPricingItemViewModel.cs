using QS.ViewModels;
using System;
using Vodovoz.Domain.Goods;
using Vodovoz.Models;

namespace Vodovoz.ViewModels.Dialogs.Goods
{
	public class NomenclatureGroupPricingItemViewModel : ViewModelBase, INomenclatureGroupPricingItemViewModel
	{
		private readonly NomenclatureGroupPricingPriceModel _groupNomenclaturePriceModel;

		public NomenclatureGroupPricingItemViewModel(NomenclatureGroupPricingPriceModel groupNomenclaturePriceModel, NomenclatureGroupPricingProductGroupViewModel group)
		{
			_groupNomenclaturePriceModel = groupNomenclaturePriceModel ?? throw new ArgumentNullException(nameof(groupNomenclaturePriceModel));
			Group = group ?? throw new ArgumentNullException(nameof(group));
			_groupNomenclaturePriceModel.PropertyChanged += GroupNomenclaturePriceModelPropertyChanged;
		}

		private void GroupNomenclaturePriceModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(_groupNomenclaturePriceModel.IsValidCostPrice):
					OnPropertyChanged(nameof(InvalidCostPrice));
					break;
				case nameof(_groupNomenclaturePriceModel.IsValidInnerDeliveryPrice):
					OnPropertyChanged(nameof(InvalidInnerDeliveryPrice));
					break;
				case nameof(_groupNomenclaturePriceModel.CostPrice):
					OnPropertyChanged(nameof(CostPrice));
					break;
				case nameof(_groupNomenclaturePriceModel.InnerDeliveryPrice):
					OnPropertyChanged(nameof(InnerDeliveryPrice));
					break;
				default:
					break;
			}
		}

		public NomenclatureGroupPricingProductGroupViewModel Group { get; }

		public bool IsGroup => false;

		public string Name => _groupNomenclaturePriceModel.Nomenclature.Name;

		public Nomenclature Nomenclature => _groupNomenclaturePriceModel.Nomenclature;

		public bool InvalidCostPrice => !_groupNomenclaturePriceModel.IsValidCostPrice;

		public decimal CostPrice
		{
			get
			{
				if(_groupNomenclaturePriceModel.CostPrice == null)
				{
					return 0;
				}
				return _groupNomenclaturePriceModel.CostPrice.Value;
			}

			set
			{
				if(value == 0)
				{
					_groupNomenclaturePriceModel.CostPrice = null;
				}
				else
				{
					_groupNomenclaturePriceModel.CostPrice = value;
				}
			}
		}

		public bool InvalidInnerDeliveryPrice => !_groupNomenclaturePriceModel.IsValidInnerDeliveryPrice;

		public decimal InnerDeliveryPrice
		{
			get
			{
				if(_groupNomenclaturePriceModel.InnerDeliveryPrice == null)
				{
					return 0;
				}
				return _groupNomenclaturePriceModel.InnerDeliveryPrice.Value;
			}

			set
			{
				if(value == 0)
				{
					_groupNomenclaturePriceModel.InnerDeliveryPrice = null;
				}
				else
				{
					_groupNomenclaturePriceModel.InnerDeliveryPrice = value;
				}
			}
		}
	}
}
