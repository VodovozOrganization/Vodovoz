using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Logistic.DriversStopLists
{
	public class DriverStopListRemovalViewModel : UowDialogViewModelBase
	{
		DriverStopListRemoval _driverStopListRemoval = new DriverStopListRemoval();
		private readonly CommonServices _commonServices;
		private readonly IEmployeeService _employeeService;
		private int _selectedPeriodInHours;

		private DelegateCommand _createCommand;
		private DelegateCommand _cancelCommand;
		private DelegateCommand _setSelectedPeriod1HourCommand;
		private DelegateCommand _setSelectedPeriod3HourCommand;
		private DelegateCommand _setSelectedPeriod24HoursCommand;

		public DriverStopListRemovalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			CommonServices commonServices,
			IEmployeeService employeeService,
			int driverId
			) : base(unitOfWorkFactory, navigation)
		{
			_commonServices = commonServices ?? throw new System.ArgumentNullException(nameof(commonServices));
			_employeeService = employeeService ?? throw new System.ArgumentNullException(nameof(employeeService));

			_driverStopListRemoval.Driver = GetDriverById(driverId);
			_driverStopListRemoval.Author = GetCurrentuser();
		}

		private Employee GetDriverById(int id)
		{
			return UoW.GetById<Employee>(id);
		}

		private Employee GetCurrentuser()
		{
			return  _employeeService.GetEmployeeForUser(UoW, ServicesConfig.UserService.CurrentUserId);
		}

		public DriverStopListRemoval DriverStopListRemoval => _driverStopListRemoval;

		public string DriverInfo => $"Вы хотите снять стоп-лист с " +
			$"{DriverStopListRemoval.Driver?.LastName} " +
			$"{DriverStopListRemoval.Driver?.Name} " +
			$"{DriverStopListRemoval.Driver?.Patronymic}";

		#region Commands

		#region CreateCommand
		public DelegateCommand CreateCommand
		{
			get
			{
				if(_createCommand == null)
				{
					_createCommand = new DelegateCommand(Create, () => CanCreate);
					_createCommand.CanExecuteChangedWith(this, x => x.CanCreate);
				}
				return _createCommand;
			}
		}

		public bool CanCreate => true;

		private void Create()
		{
			if(DriverStopListRemoval?.Driver == null)
			{
				_commonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Warning, "Водитель не найден");
			}

			if(DriverStopListRemoval?.Author == null)
			{
				_commonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Warning, "Текущий пользователь не найден");
			}

			if(string.IsNullOrWhiteSpace(DriverStopListRemoval?.Comment))
			{
				_commonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Warning, "Не добавлен комментарий");
			}

			if(_selectedPeriodInHours == 0)
			{
				_commonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Warning, "Не выбран период снятия стоп-листа");
			}

			if(DriverStopListRemoval.Driver.IsDriverHasActiveStopListRemoval(UoW))
			{
				_commonServices.InteractiveService.ShowMessage(QS.Dialog.ImportanceLevel.Warning, "У данного водителя уже снят стоп-лист");
			}

			DriverStopListRemoval.DateFrom = DateTime.Now;
			DriverStopListRemoval.DateTo = DateTime.Now.AddHours(_selectedPeriodInHours);

			UoW.Save(DriverStopListRemoval);

			Close(false, CloseSource.Cancel);
		}

		#endregion

		#region CancelCommand
		public DelegateCommand CancelCommand
		{
			get
			{
				if(_cancelCommand == null)
				{
					_cancelCommand = new DelegateCommand(Cancel, () => CanCancel);
					_cancelCommand.CanExecuteChangedWith(this, x => x.CanCancel);
				}
				return _cancelCommand;
			}
		}

		public bool CanCancel => true;

		private void Cancel()
		{
			Close(false, CloseSource.Cancel);
		}

		#endregion

		#region SetSelectedPeriod1HourCommand
		public DelegateCommand SetSelectedPeriod1HourCommand
		{
			get
			{
				if(_setSelectedPeriod1HourCommand == null)
				{
					_setSelectedPeriod1HourCommand = new DelegateCommand(SetSelectedPeriod1Hour, () => CanSetSelectedPeriod1Hour);
					_setSelectedPeriod1HourCommand.CanExecuteChangedWith(this, x => x.CanSetSelectedPeriod1Hour);
				}
				return _setSelectedPeriod1HourCommand;
			}
		}

		public bool CanSetSelectedPeriod1Hour => true;

		private void SetSelectedPeriod1Hour()
		{
			_selectedPeriodInHours = 1;
		}

		#endregion

		#region SetSelectedPeriod3HourCommand
		public DelegateCommand SetSelectedPeriod3HourCommand
		{
			get
			{
				if(_setSelectedPeriod3HourCommand == null)
				{
					_setSelectedPeriod3HourCommand = new DelegateCommand(SetSelectedPeriod3Hour, () => CanSetSelectedPeriod3Hour);
					_setSelectedPeriod3HourCommand.CanExecuteChangedWith(this, x => x.CanSetSelectedPeriod3Hour);
				}
				return _setSelectedPeriod3HourCommand;
			}
		}

		public bool CanSetSelectedPeriod3Hour => true;

		private void SetSelectedPeriod3Hour()
		{

		}

		#endregion

		#region SetSelectedPeriod24HourCommand
		public DelegateCommand SetSelectedPeriod24HoursCommand
		{
			get
			{
				if(_setSelectedPeriod24HoursCommand == null)
				{
					_setSelectedPeriod24HoursCommand = new DelegateCommand(SetSelectedPeriod24Hours, () => CanSetSelectedPeriod24Hours);
					_setSelectedPeriod24HoursCommand.CanExecuteChangedWith(this, x => x.CanSetSelectedPeriod24Hours);
				}
				return _setSelectedPeriod24HoursCommand;
			}
		}

		public bool CanSetSelectedPeriod24Hours => true;

		private void SetSelectedPeriod24Hours()
		{
			_selectedPeriodInHours = 24;
		}

		#endregion

		#endregion
	}
}
