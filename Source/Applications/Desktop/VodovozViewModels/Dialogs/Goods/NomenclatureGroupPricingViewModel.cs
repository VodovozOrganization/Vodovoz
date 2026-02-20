using Gamma.Binding.Core.LevelTreeConfig;
using QS.Commands;
using QS.Dialog;
using QS.Navigation;
using QS.Project.Domain;
using QS.Validation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Models;

namespace Vodovoz.ViewModels.Dialogs.Goods
{
	public class NomenclatureGroupPricingViewModel : DialogViewModelBase
	{
		private readonly INomenclatureGroupPricingModel _nomenclatureGroupPricingModel;
		private readonly IValidator _validator;
		private readonly IInteractiveMessage _interactiveMessage;
		private readonly Dictionary<int, NomenclatureGroupPricingProductGroupViewModel> _productGroupViewModels;
		private IList<NomenclatureGroupPricingProductGroupViewModel> _priceViewModels;
		private INomenclatureGroupPricingItemViewModel _selectedItemViewModel;
		private DelegateCommand _saveCommand;
		private DelegateCommand _closeCommand;
		private DelegateCommand _openNomenclatureCommand;
		private DateTime _date;

		public NomenclatureGroupPricingViewModel(
			INomenclatureGroupPricingModel nomenclatureGroupPricingModel,
			IValidator validator,
			IInteractiveMessage interactiveMessage,
			INavigationManager navigation) : base(navigation)
		{
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
			_interactiveMessage = interactiveMessage ?? throw new ArgumentNullException(nameof(interactiveMessage));
			_nomenclatureGroupPricingModel = nomenclatureGroupPricingModel ?? throw new ArgumentNullException(nameof(nomenclatureGroupPricingModel));
			_productGroupViewModels = new Dictionary<int, NomenclatureGroupPricingProductGroupViewModel>();
			_priceViewModels = new List<NomenclatureGroupPricingProductGroupViewModel>();

			Title = "Групповое заполнение себестоимости";

			LevelConfig = LevelConfigFactory.FirstLevel<NomenclatureGroupPricingProductGroupViewModel, NomenclatureGroupPricingItemViewModel>(group => group.PriceViewModels)
				.LastLevel(price => price.Group).EndConfig();

			Date = DateTime.Today;
		}

		public virtual IList<NomenclatureGroupPricingProductGroupViewModel> PriceViewModels
		{
			get => _priceViewModels;
			set => SetField(ref _priceViewModels, value);
		}

		public virtual INomenclatureGroupPricingItemViewModel SelectedItemViewModel
		{
			get => _selectedItemViewModel;
			set
			{
				if(SetField(ref _selectedItemViewModel, value))
				{
					OnPropertyChanged(nameof(CanOpenNomenclature));
				}
			}
		}

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
			_productGroupViewModels.Clear();
			_nomenclatureGroupPricingModel.LoadPrices(_date);

			foreach(var priceModel in _nomenclatureGroupPricingModel.PriceModels)
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

		private NomenclatureGroupPricingProductGroupViewModel GetProductGroupViewModel(NomenclatureGroupPricingPriceModel groupNomenclaturePriceModel)
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
			var isValid = _validator.Validate(_nomenclatureGroupPricingModel, showValidationResults: false);
			if(!isValid)
			{
				_interactiveMessage.ShowMessage(ImportanceLevel.Warning, "По красным ячейкам уже существует версия стоимости с датой равной или большей. " +
					"Сотрите значение в красной ячейке или измените версию в карточке товара");
				return;
			}
			_nomenclatureGroupPricingModel.SavePrices();
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

		#region Open nomenclature command

		public DelegateCommand OpenNomenclatureCommand
		{
			get
			{
				if(_openNomenclatureCommand == null)
				{
					_openNomenclatureCommand = new DelegateCommand(OpenNomenclature, () => CanOpenNomenclature);
					_openNomenclatureCommand.CanExecuteChangedWith(this, x => x.CanOpenNomenclature);
				}
				return _openNomenclatureCommand;
			}
		}

		public bool CanOpenNomenclature => SelectedItemViewModel != null && SelectedItemViewModel is NomenclatureGroupPricingItemViewModel;

		private void OpenNomenclature()
		{
			var selectedItem = (NomenclatureGroupPricingItemViewModel)SelectedItemViewModel;
			var uowBuilder = EntityUoWBuilder.ForOpen(selectedItem.Nomenclature.Id);
			NavigationManager.OpenViewModel<NomenclatureViewModel, IEntityUoWBuilder>(null, uowBuilder);
		}

		#endregion
	}
}
