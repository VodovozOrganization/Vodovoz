using QS.Commands;
using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.Models;
using VodovozInfrastructure.Observable;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class NomenclaturePurchasePricesViewModel : WidgetViewModelBase
	{
		private readonly Nomenclature _entity;
		private readonly INomenclaturePurchasePriceModel _nomenclaturePurchasePriceModel;
		private readonly IReadyObservableListBinding _pricesBinding;
		private GenericObservableList<NomenclaturePurchasePriceViewModel> _priceViewModels = new GenericObservableList<NomenclaturePurchasePriceViewModel>();
		private NomenclaturePurchasePriceViewModel _selectedPrice;


		private DateTime? _startDate;
		private DelegateCommand _changeDateCommand;
		private DelegateCommand _createPriceCommand;

		public NomenclaturePurchasePricesViewModel(Nomenclature entity, INomenclaturePurchasePriceModel nomenclaturePurchasePriceModel)
		{
			_entity = entity ?? throw new ArgumentNullException(nameof(entity));
			_nomenclaturePurchasePriceModel = nomenclaturePurchasePriceModel ?? throw new ArgumentNullException(nameof(nomenclaturePurchasePriceModel));

			_pricesBinding = ObservableListBinder.Bind(entity.ObservablePurchasePrices).To(PriceViewModels, CreatePriceViewModel);
			PriceViewModels.ListContentChanged += PriceViewModels_ListContentChanged;
		}

		private NomenclaturePurchasePriceViewModel CreatePriceViewModel(NomenclaturePurchasePrice price)
		{
			return new NomenclaturePurchasePriceViewModel(price);
		}

		private void PriceViewModels_ListContentChanged(object sender, EventArgs e)
		{
			CreatePriceCommand.RaiseCanExecuteChanged();
			ChangeDateCommand.RaiseCanExecuteChanged();
		}

		public virtual GenericObservableList<NomenclaturePurchasePriceViewModel> PriceViewModels
		{
			get => _priceViewModels;
			private set => SetField(ref _priceViewModels, value);
		}

		public virtual NomenclaturePurchasePriceViewModel SelectedPrice
		{
			get => _selectedPrice;
			set => SetField(ref _selectedPrice, value);
		}

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set
			{
				if(SetField(ref _startDate, value, () => StartDate))
				{
					OnPropertyChanged(nameof(CanAddPrice));
					OnPropertyChanged(nameof(CanChangeDate));
				}
			}
		}

		#region Create price

		public DelegateCommand CreatePriceCommand
		{
			get
			{
				if(_createPriceCommand == null)
				{
					_createPriceCommand = new DelegateCommand(CreatePrice, () => CanAddPrice);
					_createPriceCommand.CanExecuteChangedWith(this, x => x.CanAddPrice);
				}

				return _createPriceCommand;
			}
		}

		private bool CanAddPrice => StartDate.HasValue
			&& _nomenclaturePurchasePriceModel.CanCreatePrice(_entity, StartDate.Value);

		private void CreatePrice()
		{
			_nomenclaturePurchasePriceModel.CreatePrice(_entity, StartDate.Value);
		}

		#endregion Create price

		#region Change date

		public DelegateCommand ChangeDateCommand
		{
			get
			{
				if(_changeDateCommand == null)
				{
					_changeDateCommand = new DelegateCommand(ChangeDate, () => CanChangeDate);
					_changeDateCommand.CanExecuteChangedWith(this, x => x.CanChangeDate, x => x.SelectedPrice);
				}

				return _changeDateCommand;
			}
		}

		private bool CanChangeDate => StartDate.HasValue 
			&& SelectedPrice != null
			&& _nomenclaturePurchasePriceModel.CanChangeDate(_entity, SelectedPrice.Entity, StartDate.Value);

		private void ChangeDate()
		{
			
			_nomenclaturePurchasePriceModel.ChangeDate(_entity, SelectedPrice.Entity, StartDate.Value);
		}

		#endregion Change date
	}
}
