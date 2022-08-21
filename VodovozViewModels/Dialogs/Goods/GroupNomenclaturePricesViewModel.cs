using Gamma.Binding.Core.LevelTreeConfig;
using QS.Commands;
using QS.Dialog;
using QS.Navigation;
using QS.Validation;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using System;
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
		private readonly IInteractiveMessage _interactiveMessage;
		private DelegateCommand _saveCommand;
		private DelegateCommand _closeCommand;
		private DateTime _date;
		Dictionary<int, NomenclatureGroupPricingProductGroupViewModel> _productGroupViewModels = new Dictionary<int, NomenclatureGroupPricingProductGroupViewModel>();

		public NomenclatureGroupPricingViewModel(
			GroupNomenclaturePricesModel groupNomenclaturePriceModel,
			IValidator validator,
			IInteractiveMessage interactiveMessage,
			INavigationManager navigation) : base(navigation)
		{
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
			_interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			_groupNomenclaturePriceModel = groupNomenclaturePriceModel ?? throw new ArgumentNullException(nameof(groupNomenclaturePriceModel));

			Title = "Групповое заполнение себестоимости";

			LevelConfig = LevelConfigFactory.FirstLevel<NomenclatureGroupPricingProductGroupViewModel, NomenclatureGroupPricingItemViewModel>(group => group.PriceViewModels)
				.LastLevel(price => price.Group).EndConfig();

			Date = DateTime.Today;
		}

		public IList<NomenclatureGroupPricingProductGroupViewModel> PriceViewModels { get; private set; } = new List<NomenclatureGroupPricingProductGroupViewModel>();

		public ILevelConfig[] LevelConfig { get; }
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
				var productGroupViewModel = GetProductGroupViewModel(priceModel);
				var priceViewModel = new NomenclatureGroupPricingItemViewModel(priceModel, productGroupViewModel);
				productGroupViewModel.AddPriceViewModel(priceViewModel);
			}

			PriceViewModels = GetOrderedViewModels();
		}

		private IList<NomenclatureGroupPricingProductGroupViewModel> GetOrderedViewModels()
		{
			var result = _productGroupViewModels.Where(x => x.Key != 0).Select(x => x.Value).OrderBy(x => x.Name).ToList();

			if(_productGroupViewModels.TryGetValue(0, out NomenclatureGroupPricingProductGroupViewModel withoutGroupViewModel))
			{
				result.Insert(0, withoutGroupViewModel);
			}
			return result;
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
			if(group == null)
			{
				return new ProductGroup
				{
					Id = 0,
					Name = "Без группы"
				};
			}
			groups.Add(group);
			do
			{
				group = group.Parent;
				if(group == null)
				{
					break;
				}
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
					_saveCommand = new DelegateCommand(Save);
				}
				return _saveCommand;
			}
		}

		private void Save()
		{
			var isValid = _validator.Validate(_groupNomenclaturePriceModel, showValidationResults: false);
			if(!isValid)
			{
				_interactiveMessage.ShowMessage(ImportanceLevel.Warning, "По красным ячейкам уже существует версия стоимости с датой равной или большей. " +
					"Сотрите значение в красной ячейке или измените версию в карточке товара");
				return;
			}
			_groupNomenclaturePriceModel.SavePrices();
			Close(false, CloseSource.Save);

		}

		#endregion

		#region Close command

		public DelegateCommand CloseCommand
		{
			get
			{
				if(_closeCommand == null)
				{
					_closeCommand = new DelegateCommand(Close);
				}
				return _closeCommand;
			}
		}

		private void Close()
		{
			Close(false, CloseSource.Cancel);
		}

		#endregion Close command
	}


	public class NomenclatureGroupPricingItemViewModel : ViewModelBase, INomenclatureGroupPricingItemViewModel
	{
		private readonly GroupNomenclaturePriceModel _groupNomenclaturePriceModel;

		public NomenclatureGroupPricingItemViewModel(GroupNomenclaturePriceModel groupNomenclaturePriceModel, NomenclatureGroupPricingProductGroupViewModel group)
		{
			_groupNomenclaturePriceModel = groupNomenclaturePriceModel ?? throw new ArgumentNullException(nameof(groupNomenclaturePriceModel));
			Group = group ?? throw new ArgumentNullException(nameof(group));
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

		public NomenclatureGroupPricingProductGroupViewModel Group { get; set; }

		public bool IsGroup => false;

		public string Name => _groupNomenclaturePriceModel.Nomenclature.Name;

		public bool InvalidCostPurchasePrice => !_groupNomenclaturePriceModel.IsValidCostPurchasePrice;

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
			_priceViewModels = new List<NomenclatureGroupPricingItemViewModel>();
			_productGroup = productGroup ?? throw new ArgumentNullException(nameof(productGroup));
		}

		public bool IsGroup => true;

		public string Name => _productGroup.Name;

		bool INomenclatureGroupPricingItemViewModel.InvalidCostPurchasePrice => false;

		decimal INomenclatureGroupPricingItemViewModel.CostPurchasePrice { get; set; }

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
