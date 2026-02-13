using QS.ViewModels;
using System;
using System.Linq;
using System.Reflection;
using Gamma.Utilities;
using NHibernate;
using QS.Dialog;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Widgets;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.ViewModels.ViewModels.Profitability
{
	public class ProfitabilityConstantsDataViewModel : WidgetViewModelBase
	{
		private bool _isAdministrativeExpensesProductGroupsFilterActive;
		private bool _isWarehousesExpensesProductGroupsFilterActive;
		private bool _isAdministrativeExpensesWarehousesFilterActive;
		private bool _isWarehousesExpensesWarehousesFilterActive;
		private bool _isCarEventTypesFilterActive;

		private PropertyInfo[] _filterBooleanProperties;
		
		public ProfitabilityConstantsDataViewModel(
			IUnitOfWork uow,
			ProfitabilityConstants entity,
			ISelectableParametersFilterViewModelFactory selectableParametersFilterViewModelFactory)
		{
			if(selectableParametersFilterViewModelFactory == null)
			{
				throw new ArgumentNullException(nameof(selectableParametersFilterViewModelFactory));
			}

			Entity = entity ?? throw new ArgumentException(nameof(entity));
			CreateFilters(uow, selectableParametersFilterViewModelFactory);
			
			//Получаем все булевы свойства, сейчас они все отвечают за показ фильтров
			GetBooleanPropertiesWithSetter();
		}

		public ProfitabilityConstants Entity { get; }
		public SelectableParametersFilterViewModel ActiveFilterViewModel { get; private set; }
		public SelectableParametersFilterViewModel AdministrativeExpensesProductGroupsFilterViewModel { get; private set; }
		public SelectableParametersFilterViewModel AdministrativeExpensesWarehousesFilterViewModel { get; private set; }
		public SelectableParametersFilterViewModel WarehouseExpensesProductGroupsFilterViewModel { get; private set; }
		public SelectableParametersFilterViewModel WarehouseExpensesWarehousesFilterViewModel { get; private set; }
		public SelectableParametersFilterViewModel CarEventsFilterViewModel { get; private set; }
		public IProgressBarDisplayable ProgressBarDisplayable { get; set; }

		public bool IsAdministrativeExpensesProductGroupsFilterActive
		{
			get => _isAdministrativeExpensesProductGroupsFilterActive;
			set
			{
				if(SetField(ref _isAdministrativeExpensesProductGroupsFilterActive, value) && value)
				{
					ChangeFilterPropertiesToFalse(nameof(IsAdministrativeExpensesProductGroupsFilterActive));
				}
			}
		}
		
		public bool IsAdministrativeExpensesWarehousesFilterActive
		{
			get => _isAdministrativeExpensesWarehousesFilterActive;
			set
			{
				if(SetField(ref _isAdministrativeExpensesWarehousesFilterActive, value) && value)
				{
					ChangeFilterPropertiesToFalse(nameof(IsAdministrativeExpensesWarehousesFilterActive));
				}
			}
		}
		
		public bool IsWarehousesExpensesProductGroupsFilterActive
		{
			get => _isWarehousesExpensesProductGroupsFilterActive;
			set
			{
				if(SetField(ref _isWarehousesExpensesProductGroupsFilterActive, value) && value)
				{
					ChangeFilterPropertiesToFalse(nameof(IsWarehousesExpensesProductGroupsFilterActive));
				}
			}
		}
		
		public bool IsWarehousesExpensesWarehousesFilterActive
		{
			get => _isWarehousesExpensesWarehousesFilterActive;
			set
			{
				if(SetField(ref _isWarehousesExpensesWarehousesFilterActive, value) && value)
				{
					ChangeFilterPropertiesToFalse(nameof(IsWarehousesExpensesWarehousesFilterActive));
				}
			}
		}
		
		public bool IsCarEventTypesFilterActive
		{
			get => _isCarEventTypesFilterActive;
			set
			{
				if(SetField(ref _isCarEventTypesFilterActive, value) && value)
				{
					ChangeFilterPropertiesToFalse(nameof(IsCarEventTypesFilterActive));
				}
			} 
		}
		
		public bool IsCalculationDateAndAuthorActive => Entity.Id != 0;

		public bool UpdateActiveFilterViewModel(SelectableParametersFilterViewModel filterViewModel)
		{
			if(ActiveFilterViewModel != null && ActiveFilterViewModel == filterViewModel)
			{
				return false;
			}

			ActiveFilterViewModel = filterViewModel;
			return true;
		}

		public void FirePropertyChanged(string propertyName)
		{
			OnPropertyChanged(propertyName);
		}

		private void ChangeFilterPropertiesToFalse(string excludeProperty)
		{
			foreach(var propInfo in _filterBooleanProperties)
			{
				if(propInfo.Name == excludeProperty)
				{
					continue;
				}

				this.SetPropertyValue(propInfo.Name, false);
			}
		}

		private void CreateFilters(
			IUnitOfWork uow,
			ISelectableParametersFilterViewModelFactory selectableParametersFilterViewModelFactory)
		{
			CreateProductGroupsFilters(uow, selectableParametersFilterViewModelFactory);
			CreateWarehousesFilters(uow, selectableParametersFilterViewModelFactory);
			CreateCarEventTypesFilter(uow, selectableParametersFilterViewModelFactory);
		}

		private void CreateProductGroupsFilters(
			IUnitOfWork uow,
			ISelectableParametersFilterViewModelFactory selectableParametersFilterViewModelFactory)
		{
			ProductGroup productGroupChildAlias = null;
			var query = uow.Session.QueryOver<ProductGroup>()
				.Left.JoinAlias(p => p.Childs, () => productGroupChildAlias);

			if(uow.IsNew)
			{
				query.Where(() => !productGroupChildAlias.IsArchive);
			}
			
			query.Fetch(SelectMode.Fetch,p => p.Childs)
				.List();
			
			AdministrativeExpensesProductGroupsFilterViewModel =
				selectableParametersFilterViewModelFactory.CreateProductGroupsSelectableParametersFilterViewModel(
					uow, "Фильтр по группам товаров(ОХР)");
			WarehouseExpensesProductGroupsFilterViewModel =
				selectableParametersFilterViewModelFactory.CreateProductGroupsSelectableParametersFilterViewModel(
					uow, "Фильтр по группам товаров(складские расходы)");
			
			AdministrativeExpensesProductGroupsFilterViewModel.SelectParameters(Entity.AdministrativeProductGroupsFilter);
			WarehouseExpensesProductGroupsFilterViewModel.SelectParameters(Entity.WarehouseExpensesProductGroupsFilter);
		}
		
		private void CreateWarehousesFilters(
			IUnitOfWork uow,
			ISelectableParametersFilterViewModelFactory selectableParametersFilterViewModelFactory)
		{
			AdministrativeExpensesWarehousesFilterViewModel = 
				selectableParametersFilterViewModelFactory.CreateWarehousesSelectableParametersFilterViewModel(
					uow, "Фильтр по складам(ОХР)");
			WarehouseExpensesWarehousesFilterViewModel =
				selectableParametersFilterViewModelFactory.CreateWarehousesSelectableParametersFilterViewModel(
					uow, "Фильтр по складам(складские расходы)");
			
			AdministrativeExpensesWarehousesFilterViewModel.SelectParameters(Entity.AdministrativeWarehousesFilter);
			WarehouseExpensesWarehousesFilterViewModel.SelectParameters(Entity.WarehouseExpensesWarehousesFilter);
		}
		
		private void CreateCarEventTypesFilter(
			IUnitOfWork uow,
			ISelectableParametersFilterViewModelFactory selectableParametersFilterViewModelFactory)
		{
			CarEventsFilterViewModel =
				selectableParametersFilterViewModelFactory.CreateCarEventTypesSelectableParametersFilterViewModel(
					uow, "Фильтр по типам событий ТС");
			
			CarEventsFilterViewModel.SelectParameters(Entity.RepairCostCarEventTypesFilter);
		}

		private void GetBooleanPropertiesWithSetter()
		{
			_filterBooleanProperties =
				typeof(ProfitabilityConstantsDataViewModel).GetProperties()
					.Where(x => x.PropertyType == typeof(bool) && x.GetSetMethod() != null).ToArray();
		}
	}
}
