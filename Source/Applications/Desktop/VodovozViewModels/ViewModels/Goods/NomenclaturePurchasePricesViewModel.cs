using QS.Commands;
using QS.ViewModels;
using System;
using System.Collections.Specialized;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Extensions.Observable.Collections.List;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Models;

namespace Vodovoz.ViewModels.ViewModels.Goods
{
	public class NomenclaturePurchasePricesViewModel : WidgetViewModelBase, IDisposable
	{
		private readonly Nomenclature _entity;
		private readonly INomenclaturePurchasePriceModel _nomenclaturePurchasePriceModel;
		private NomenclaturePurchasePriceViewModel _selectedPrice;


		private DateTime? _startDate;
		private DelegateCommand _changeDateCommand;
		private DelegateCommand _createPriceCommand;

		public NomenclaturePurchasePricesViewModel(Nomenclature entity, INomenclaturePurchasePriceModel nomenclaturePurchasePriceModel)
		{
			_entity = entity ?? throw new ArgumentNullException(nameof(entity));
			_nomenclaturePurchasePriceModel = nomenclaturePurchasePriceModel ?? throw new ArgumentNullException(nameof(nomenclaturePurchasePriceModel));

			entity.PurchasePrices.CollectionChanged += OnObservablePurchasePricesElementAdded;

			if(entity.PurchasePrices.Any())
			{
				var sortedList = entity.PurchasePrices.OrderByDescending(x => x.StartDate);
				foreach(var item in sortedList)
				{
					PriceViewModels.Add(CreatePriceViewModel(item));
				}
			}
		}

		private void OnObservablePurchasePricesElementAdded(object sender, NotifyCollectionChangedEventArgs e)
		{
			if(e.Action == NotifyCollectionChangedAction.Add)
			{
				PriceViewModels.Insert(0, CreatePriceViewModel(_entity.PurchasePrices.Last()));
			}
		}

		private NomenclaturePurchasePriceViewModel CreatePriceViewModel(NomenclaturePurchasePrice price)
		{
			return new NomenclaturePurchasePriceViewModel(price);
		}

		public virtual GenericObservableList<NomenclaturePurchasePriceViewModel> PriceViewModels { get;} =
			new GenericObservableList<NomenclaturePurchasePriceViewModel>();

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

		public void Dispose()
		{
			_entity.PurchasePrices.CollectionChanged -= OnObservablePurchasePricesElementAdded;
		}
	}
}
