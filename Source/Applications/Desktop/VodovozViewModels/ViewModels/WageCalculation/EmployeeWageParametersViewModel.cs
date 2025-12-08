using System;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Employees;
using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.Tdi;
using QS.DomainModel.UoW;
using Vodovoz.Domain.WageCalculation;
using QS.DomainModel.Entity.PresetPermissions;
using Vodovoz.EntityRepositories;
using QS.DomainModel.Entity;
using QS.Navigation;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.WageCalculation;

namespace Vodovoz.ViewModels.WageCalculation
{
	public class EmployeeWageParametersViewModel : EntityWidgetViewModelBase<Employee>
	{
		private readonly ITdiTab tab;
		private readonly ICommonServices commonServices;
		private readonly INavigationManager navigationManager;
		private readonly IWageCalculationRepository _wageCalculationRepository;
		private readonly bool canChangeWageCalculation;
		private readonly bool _canEditWageBySelfSubdivision;

		public EmployeeWageParametersViewModel(
			Employee entity, 
			ITdiTab tab,
			IUnitOfWork uow, 
			IPresetPermissionValidator permissionValidator,
			IUserRepository userRepository, 
			ICommonServices commonServices,
			INavigationManager navigationManager, 
			IEmployeeRepository employeeRepository,
			IWageCalculationRepository wageCalculationRepository) : base(entity, commonServices)
		{
			this.tab = tab ?? throw new ArgumentNullException(nameof(tab));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.navigationManager = navigationManager;
			_wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			Entity.ObservableWageParameters.ElementAdded += (aList, aIdx) => WageParametersUpdated();
			Entity.ObservableWageParameters.ElementRemoved += (aList, aIdx, aObject) => WageParametersUpdated();
			canChangeWageCalculation =
				permissionValidator.Validate(Vodovoz.Core.Domain.Permissions.EmployeePermissions.CanEditWage, userRepository.GetCurrentUser(UoW).Id);
			_canEditWageBySelfSubdivision = userRepository.GetCurrentUser(UoW).IsAdmin ||
										   (employeeRepository.GetEmployeeForCurrentUser(UoW).Subdivision == Entity.Subdivision &&
										    permissionValidator.Validate("can_edit_wage_by_self_subdivision", userRepository.GetCurrentUser(UoW).Id)
										    );
		}

		public event EventHandler OnParameterNodesUpdated;

		private DateTime? startDate;
		[PropertyChangedAlso(nameof(CanChangeWageCalculation))]
		public virtual DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value, () => StartDate);
		}

		public virtual IList<EmployeeWageParameterNode> WageParameterNodes => Entity.ObservableWageParameters
			.Select(x => new EmployeeWageParameterNode(x)).ToList();


		private void WageParametersUpdated()
		{
			OnParameterNodesUpdated?.Invoke(this, EventArgs.Empty);
		}


		#region ChangeWageParameterCommand

		public virtual bool CanChangeWageCalculation =>
			(StartDate.HasValue && Entity.CheckStartDateForNewWageParameter(StartDate.Value)) &&
			(canChangeWageCalculation || _canEditWageBySelfSubdivision);

		private DelegateCommand changeWageParameterCommand;

		public DelegateCommand ChangeWageParameterCommand {
			get {
				if(changeWageParameterCommand == null) {
					changeWageParameterCommand = new DelegateCommand(
						() => {
							var newEmployeeWageParameterViewModel =
								new EmployeeWageParameterViewModel(UoW, Entity, CommonServices, _wageCalculationRepository);
							newEmployeeWageParameterViewModel.OnWageParameterCreated += (sender, wageParameter) => {
								Entity.ChangeWageParameter(wageParameter, StartDate.Value);
							};
							tab.TabParent.AddSlaveTab(tab, newEmployeeWageParameterViewModel);
						},
						() => CanChangeWageCalculation
					);
					changeWageParameterCommand.CanExecuteChangedWith(this, x => x.CanChangeWageCalculation);
				}

				return changeWageParameterCommand;
			}
		}

		#endregion ChangeWageParameterCommand

		#region ChangeWageStartDateCommand

		private DelegateCommand<EmployeeWageParameterNode> changeWageStartDateCommand;

		public DelegateCommand<EmployeeWageParameterNode> ChangeWageStartDateCommand {
			get {
				if(changeWageStartDateCommand == null) {
					changeWageStartDateCommand = new DelegateCommand<EmployeeWageParameterNode>(
						(node) => {
							if(!commonServices.InteractiveService.Question(
								"Внимание! Будет произведено изменение даты в уже имеющемся расчете зарплаты, " +
								"документы попадающие в этот интервал будут пересчитываться по другому расчету! " +
								"Продолжить?", "Внимание!")) {
								return;
							}

							var previousParameter = GetPreviousParameter(node.EmployeeWageParameter.StartDate);
							if(previousParameter != null) {
								previousParameter.EndDate = StartDate.Value.AddTicks(-1);
							}
							node.EmployeeWageParameter.StartDate = StartDate.Value;
							WageParametersUpdated();
						},
						(node) => {
							if(node == null || !StartDate.HasValue) {
								return false;
							}
							var previousParameterByDate = GetPreviousParameter(StartDate.Value);
							var previousParameterBySelectedParameter = GetPreviousParameter(node.EmployeeWageParameter.StartDate);

							bool noConflictWithEndDate = !node.EmployeeWageParameter.EndDate.HasValue || node.EmployeeWageParameter.EndDate.Value > StartDate;
							bool noConflictWithPreviousStartDate = (previousParameterByDate == null && previousParameterBySelectedParameter == null) || (previousParameterBySelectedParameter != null && previousParameterBySelectedParameter.StartDate < StartDate);

							return StartDate.HasValue && noConflictWithEndDate && noConflictWithPreviousStartDate;
						}
					);
					changeWageStartDateCommand.CanExecuteChangedWith(this, x => x.StartDate);
				}

				return changeWageStartDateCommand;
			}
		}

		private WageParameter GetPreviousParameter(DateTime date)
		{
			return Entity.ObservableWageParameters
						.Where(x => x.EndDate != null)
						.Where(x => x.EndDate <= date)
						.OrderByDescending(x => x.EndDate)
						.FirstOrDefault();
		}

		#endregion ChangeWageStartDateCommand

		#region OpenWageParameterCommand

		private DelegateCommand<EmployeeWageParameterNode> openWageParameterCommand;

		public DelegateCommand<EmployeeWageParameterNode> OpenWageParameterCommand {
			get {
				if(openWageParameterCommand == null) {
					openWageParameterCommand = new DelegateCommand<EmployeeWageParameterNode>(
						(node) => {
							EmployeeWageParameterViewModel employeeWageParameterViewModel =
								new EmployeeWageParameterViewModel(
									UoW, node.EmployeeWageParameter, CommonServices, _wageCalculationRepository);
							tab.TabParent.AddTab(employeeWageParameterViewModel, tab);
						},
						(node) => node != null
					);
				}

				return openWageParameterCommand;
			}
		}
		
		#endregion OpenWageParameterCommand
	}
}
