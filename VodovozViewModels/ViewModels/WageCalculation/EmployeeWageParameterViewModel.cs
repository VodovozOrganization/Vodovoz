using System;
using Gamma.Utilities;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.WageCalculation;
using QS.Navigation;
using QS.Project.Domain;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.WageCalculation
{
	//Наследуется от TabViewModelBase потому что должен и открываться во вкладке и иметь существующий UoW а не создавать новый
	public class EmployeeWageParameterViewModel : TabViewModelBase, ISingleUoWDialog
	{
		private readonly ICommonServices commonServices;
		public IUnitOfWork UoW { get; }

		private readonly EmployeeWageParameter entity;
		public event EventHandler<EmployeeWageParameter> OnWageParameterCreated;
		private readonly bool isNewEntity;

		public EmployeeWageParameterViewModel(IUnitOfWork uow, Employee employee, ICommonServices commonServices) 
			: base((commonServices ?? throw new ArgumentNullException(nameof(commonServices))).InteractiveService, null)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			if(employee == null) throw new ArgumentNullException(nameof(employee));
			entity = new EmployeeWageParameter();
			entity.Employee = employee;
			this.commonServices = commonServices;
			isNewEntity = true;
			Title = $"Параметры расчета зарплаты ({entity.Employee.GetPersonNameWithInitials()})";
			OpenWageParameterItemViewModel();
		}
		
		public EmployeeWageParameterViewModel(IUnitOfWork uow, EmployeeWageParameter employeeWageParameter, ICommonServices commonServices) 
			: base((commonServices ?? throw new ArgumentNullException(nameof(commonServices))).InteractiveService, null)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			entity = employeeWageParameter ?? throw new ArgumentNullException(nameof(employeeWageParameter));
			this.commonServices = commonServices;
			Title = $"Параметры расчета зарплаты ({entity.Employee.GetPersonNameWithInitials()})";
			OpenWageParameterItemViewModel();
		}

		public bool CanEdit => isNewEntity;

		private WageParameterItemTypes wageParameterItemType;
		public virtual WageParameterItemTypes WageParameterItemType {
			get => wageParameterItemType;
			set {
				if(SetField(ref wageParameterItemType, value)) {
					OpenWageParameterItemViewModel();
				}
			}
		}

		private WidgetViewModelBase wageParameterItemViewModel;
		public virtual WidgetViewModelBase WageParameterItemViewModel {
			get => wageParameterItemViewModel;
			set => SetField(ref wageParameterItemViewModel, value);
		}

		private WidgetViewModelBase driverWithCompanyCarWageParameterItemViewModel;
		public virtual WidgetViewModelBase DriverWithCompanyCarWageParameterItemViewModel {
			get => driverWithCompanyCarWageParameterItemViewModel;
			set => SetField(ref driverWithCompanyCarWageParameterItemViewModel, value);
		}

		private DelegateCommand openWageParameterItemViewModelCommand;

		public DelegateCommand OpenWageParameterItemViewModelCommand {
			get {
				if(openWageParameterItemViewModelCommand == null) {
					openWageParameterItemViewModelCommand = new DelegateCommand(OpenWageParameterItemViewModel);
				}

				return openWageParameterItemViewModelCommand;
			}
		}
		
		private void OpenWageParameterItemViewModel()
		{
			(WageParameterItemViewModel as IDisposable)?.Dispose();

			if(isNewEntity) {
				entity.CreateWageParameterItems(WageParameterItemType);
			}
			WageParameterItemViewModel = GetWageParameterItemViewModel(entity.WageParameterItem);
			WageParameterItemType = entity.WageParameterItem.WageParameterItemType;
			DriverWithCompanyCarWageParameterItemViewModel =
				GetWageParameterItemViewModel(entity.DriverWithOurCarsWageParameterItem);
		}


		private WidgetViewModelBase GetWageParameterItemViewModel(WageParameterItem wageParameterItem)
		{
			if(wageParameterItem == null) {
				return null;
			}
			
			switch(wageParameterItem.WageParameterItemType) {
				case WageParameterItemTypes.OldRates:
					return new OldRatesWageParameterItemViewModel((OldRatesWageParameterItem) wageParameterItem, commonServices);
				case WageParameterItemTypes.Fixed:
					return new FixedWageParameterItemViewModel((FixedWageParameterItem) wageParameterItem, CanEdit, commonServices);
				case WageParameterItemTypes.Percent:
					return new PercentWageParameterItemViewModel((PercentWageParameterItem) wageParameterItem, CanEdit, commonServices);
				case WageParameterItemTypes.RatesLevel:
					return new RatesLevelWageParameterItemViewModel(UoW, (RatesLevelWageParameterItem) wageParameterItem, CanEdit, commonServices);
				case WageParameterItemTypes.SalesPlan:
					return new SalesPlanWageParameterItemViewModel(UoW, (SalesPlanWageParameterItem) wageParameterItem, CanEdit, commonServices);
				case WageParameterItemTypes.Manual:
					return null;
				default:
					throw new NotImplementedException($"Не описано какой параметер должен создаваться для типа {wageParameterItemType.GetEnumTitle()}");
			}
		}

		public void Save()
		{
			//Сохраняется только при создании, изменять параметры расчета нельзя
			if(!isNewEntity) {
				return;
			}

			if(!commonServices.ValidationService.Validate(entity)) {
				return;
			}
			
			OnWageParameterCreated?.Invoke(this, entity);
			Close(false, CloseSource.Save);
		}
	}
}
