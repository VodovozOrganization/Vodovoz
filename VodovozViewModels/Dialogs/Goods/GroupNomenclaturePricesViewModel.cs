using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Models;

namespace Vodovoz.ViewModels.Dialogs.Goods
{
	public class NomenclatureGroupPricingViewModel : DialogViewModelBase
	{
		private readonly GroupNomenclaturePricesModel _groupNomenclaturePriceModel;
		private readonly IValidator _validator;
		private DelegateCommand _saveCommand;
		private DateTime _date;
		Dictionary<int, NomenclatureGroupPricingProductGroupViewModel> _productGroupViewModels = new Dictionary<int, NomenclatureGroupPricingProductGroupViewModel>();

		public NomenclatureGroupPricingViewModel(GroupNomenclaturePricesModel groupNomenclaturePriceModel, IValidator validator, INavigationManager navigation) : base(navigation)
		{
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));

			//Модель, загружает данные и сохраняет
			_groupNomenclaturePriceModel = groupNomenclaturePriceModel ?? throw new ArgumentNullException(nameof(groupNomenclaturePriceModel));


			//Уровневая модель

			//Инициализация
			Date = DateTime.Today;
		}

		public IEnumerable<INomenclatureGroupPricingItemViewModel> PriceViewModels { get; private set; } = Enumerable.Empty<INomenclatureGroupPricingItemViewModel>();

		public virtual DateTime Date
		{
			get => _date;
			set
			{
				if(SetField(ref _date, value))
				{
					Reload();
				}
			}
		}

		private void Reload()
		{
			_groupNomenclaturePriceModel.LoadPrices(_date);

			foreach(var priceModel in _groupNomenclaturePriceModel.PriceModels)
			{
				var priceViewModel = new NomenclatureGroupPricingItemViewModel(priceModel);
				var productGroupViewModel = GetProductGroupViewModel(priceModel);
				productGroupViewModel.AddPriceViewModel(priceViewModel);
			}

			PriceViewModels = _productGroupViewModels.Select(x => x.Value);
		}

		private NomenclatureGroupPricingProductGroupViewModel GetProductGroupViewModel(GroupNomenclaturePriceModel groupNomenclaturePriceModel)
		{
			var productGroup = GetCorrectProductGroup(groupNomenclaturePriceModel.Nomenclature);
			if(!_productGroupViewModels.TryGetValue(productGroup.Id, out var productGroupViewModel))
			{
				var newViewModel = new NomenclatureGroupPricingProductGroupViewModel(productGroup);
				_productGroupViewModels.Add(productGroup.Id, newViewModel);
				return newViewModel;
			}

			return productGroupViewModel;
		}

		private ProductGroup GetCorrectProductGroup(Nomenclature nomenclature)
		{
			List<ProductGroup> groups = new List<ProductGroup>();
			ProductGroup group = nomenclature.ProductGroup;
			groups.Add(group);
			do
			{
				group = group.Parent;
				groups.Add(group);

			} while(group != null);

			if(groups.Count > 3)
			{
				return groups[2];
			}
			else if(groups.Count == 2)
			{
				return groups[1];
			}

			return groups[0];
		}

		#region Save command

		public DelegateCommand SaveCommand
		{
			get
			{
				if(_saveCommand == null)
				{
					_saveCommand = new DelegateCommand(Save, () => CanSave);
					_saveCommand.CanExecuteChangedWith(this, x => x.CanSave);
				}
				return _saveCommand;
			}
		}

		public bool CanSave => true;

		private void Save()
		{
			var isValid = _validator.Validate(_groupNomenclaturePriceModel);
			if(!isValid)
			{
				return;
			}
			_groupNomenclaturePriceModel.SavePrices();
		}

		#endregion
	}


	public class NomenclatureGroupPricingItemViewModel : ViewModelBase, INomenclatureGroupPricingItemViewModel
	{
		private readonly GroupNomenclaturePriceModel _groupNomenclaturePriceModel;

		public NomenclatureGroupPricingItemViewModel(GroupNomenclaturePriceModel groupNomenclaturePriceModel)
		{
			_groupNomenclaturePriceModel = groupNomenclaturePriceModel ?? throw new ArgumentNullException(nameof(groupNomenclaturePriceModel));
			_groupNomenclaturePriceModel.PropertyChanged += _groupNomenclaturePriceModel_PropertyChanged;
		}

		private void _groupNomenclaturePriceModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(_groupNomenclaturePriceModel.IsValidCostPurchasePrice):
					OnPropertyChanged(nameof(InvalidCostPurchasePrice));
					break;
				case nameof(_groupNomenclaturePriceModel.IsValidInnerDeliveryPrice):
					OnPropertyChanged(nameof(InvalidInnerDeliveryPrice));
					break;
				case nameof(_groupNomenclaturePriceModel.CostPurchasePrice):
					OnPropertyChanged(nameof(CostPurchasePrice));
					break;
				case nameof(_groupNomenclaturePriceModel.InnerDeliveryPrice):
					OnPropertyChanged(nameof(InnerDeliveryPrice));
					break;
				default:
					break;
			}
		}

		public bool IsProductGroup => false;

		public string Name => _groupNomenclaturePriceModel.Nomenclature.Name;

		public bool InvalidCostPurchasePrice => !_groupNomenclaturePriceModel.IsValidInnerDeliveryPrice;

		public decimal CostPurchasePrice
		{
			get
			{
				if(_groupNomenclaturePriceModel.CostPurchasePrice == null)
				{
					return 0;
				}
				return _groupNomenclaturePriceModel.CostPurchasePrice.Value;
			}

			set
			{
				if(value == 0)
				{
					_groupNomenclaturePriceModel.CostPurchasePrice = null;
				}
				else
				{
					_groupNomenclaturePriceModel.CostPurchasePrice = value;
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

	public class NomenclatureGroupPricingProductGroupViewModel : ViewModelBase, INomenclatureGroupPricingItemViewModel
	{
		private readonly ProductGroup _productGroup;
		private List<NomenclatureGroupPricingItemViewModel> _priceViewModels { get; set; }

		public NomenclatureGroupPricingProductGroupViewModel(ProductGroup productGroup)
		{
			_productGroup = productGroup ?? throw new ArgumentNullException(nameof(productGroup));
		}

		public bool IsProductGroup => true;

		public string Name => _productGroup.Name;

		bool INomenclatureGroupPricingItemViewModel.InvalidCostPurchasePrice => false;

		decimal INomenclatureGroupPricingItemViewModel.CostPurchasePrice { get; set; }

		bool INomenclatureGroupPricingItemViewModel.InvalidInnerDeliveryPrice => false;

		decimal INomenclatureGroupPricingItemViewModel.InnerDeliveryPrice { get; set; }

		public IEnumerable<NomenclatureGroupPricingItemViewModel> PriceViewModels => _priceViewModels;

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
