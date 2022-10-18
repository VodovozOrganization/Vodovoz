using QS.Commands;
using QS.ViewModels;
using System;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.Models;
using VodovozInfrastructure.Observable;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class NomenclatureCostPricesViewModel : WidgetViewModelBase
	{
		private readonly Nomenclature _entity;
		private readonly NomenclatureCostPriceModel _nomenclatureCostPriceModel;
		private readonly IReadyObservableListBinding _pricesBinding;
		private GenericObservableList<NomenclatureCostPriceViewModel> _priceViewModels = new GenericObservableList<NomenclatureCostPriceViewModel>();
		private NomenclatureCostPriceViewModel _selectedPrice;

		private DateTime? _startDate;
		private DelegateCommand _changeDateCommand;
		private DelegateCommand _createPriceCommand;

		public NomenclatureCostPricesViewModel(Nomenclature entity, NomenclatureCostPriceModel nomenclatureCostPriceModel)
		{
			_entity = entity ?? throw new ArgumentNullException(nameof(entity));
			_nomenclatureCostPriceModel = nomenclatureCostPriceModel ?? throw new ArgumentNullException(nameof(nomenclatureCostPriceModel));

			_pricesBinding = ObservableListBinder.Bind(entity.ObservableCostPrices).To(PriceViewModels, CreatePriceViewModel);
			PriceViewModels.ListContentChanged += PriceViewModels_ListContentChanged;
		}

		private NomenclatureCostPriceViewModel CreatePriceViewModel(NomenclatureCostPrice price)
		{
			return new NomenclatureCostPriceViewModel(price);
		}

		private void PriceViewModels_ListContentChanged(object sender, EventArgs e)
		{
			CreatePriceCommand.RaiseCanExecuteChanged();
			ChangeDateCommand.RaiseCanExecuteChanged();
		}

		public virtual GenericObservableList<NomenclatureCostPriceViewModel> PriceViewModels
		{
			get => _priceViewModels;
			private set => SetField(ref _priceViewModels, value);
		}

		public virtual NomenclatureCostPriceViewModel SelectedPrice
		{
			get => _selectedPrice;
			set => SetField(ref _selectedPrice, value);
		}

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set
			{
				if(SetField(ref _startDate, value))
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
			&& _nomenclatureCostPriceModel.CanCreatePrice(_entity, StartDate.Value);

		private void CreatePrice()
		{
			_nomenclatureCostPriceModel.CreatePrice(_entity, StartDate.Value);
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
			&& _nomenclatureCostPriceModel.CanChangeDate(_entity, SelectedPrice.Entity, StartDate.Value);

		private void ChangeDate()
		{
			
			_nomenclatureCostPriceModel.ChangeDate(_entity, SelectedPrice.Entity, StartDate.Value);
		}

		#endregion Change date
	}
}
