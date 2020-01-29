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

namespace Vodovoz.ViewModels.WageCalculation
{
	public class EmployeeWageParametersViewModel : EntityWidgetViewModelBase<Employee>
	{
		private readonly ITdiTab tab;
		private readonly ICommonServices commonServices;
		private readonly INavigationManager navigationManager;
		private readonly bool canChangeWageCalculation;

		public EmployeeWageParametersViewModel(
			Employee entity, 
			ITdiTab tab,
			IUnitOfWork uow, 
			IPresetPermissionValidator permissionValidator,
			IUserRepository userRepository, 
			ICommonServices commonServices,
			INavigationManager navigationManager
			) : base(entity, commonServices)
		{
			this.tab = tab ?? throw new ArgumentNullException(nameof(tab));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.navigationManager = navigationManager;
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			Entity.ObservableWageParameters.ElementAdded += (aList, aIdx) => WageParametersUpdated();
			Entity.ObservableWageParameters.ElementRemoved += (aList, aIdx, aObject) => WageParametersUpdated();
			canChangeWageCalculation = permissionValidator.Validate("can_edit_wage", userRepository.GetCurrentUser(UoW).Id);
			CreateCommands();
		}

		public event EventHandler OnParameterNodesUpdated;

		private DateTime? startDate;
		[PropertyChangedAlso(nameof(CanChangeWageCalculation))]
		public virtual DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value, () => StartDate);
		}

		public virtual IList<WageParameterNode> WageParameterNodes => Entity.ObservableWageParameters
			.Where(x => x.WageParameterTarget == WageParameterTargets.ForMercenariesCars)
			.Select(x => new WageParameterNode(x)).ToList();


		private void WageParametersUpdated()
		{
			OnParameterNodesUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void CreateCommands()
		{
			CreateChangeWageParameterCommand();
			CreateOpenWageParameterCommand();
			CreateChangeWageStartDateCommand();
		}

		#region ChangeWageParameterCommand

		public DelegateCommand ChangeWageParameterCommand { get; private set; }

		public virtual bool CanChangeWageCalculation => canChangeWageCalculation && StartDate.HasValue && Entity.CheckStartDateForNewWageParameter(StartDate.Value);

		private void CreateChangeWageParameterCommand()
		{
			ChangeWageParameterCommand = new DelegateCommand(
				() => {
					WageParameterViewModel newWageParameterViewModel = new WageParameterViewModel(UoW, WageParameterTargets.ForMercenariesCars, CommonServices, navigationManager);
					newWageParameterViewModel.OnWageParameterCreated += (sender, wageParameter) => {
						Entity.ChangeWageParameter(wageParameter, StartDate.Value);
					};
					tab.TabParent.AddSlaveTab(tab, newWageParameterViewModel);
				},
				() => CanChangeWageCalculation
			);
			ChangeWageParameterCommand.CanExecuteChangedWith(this, x => x.CanChangeWageCalculation);
		}

		#endregion ChangeWageParameterCommand

		#region ChangeWageStartDateCommand

		public DelegateCommand<WageParameterNode> ChangeWageStartDateCommand { get; private set; }

		private void CreateChangeWageStartDateCommand()
		{
			ChangeWageStartDateCommand = new DelegateCommand<WageParameterNode>(
				(node) => {
					if(!commonServices.InteractiveService.Question(
						"Внимание! Будет произведено изменение даты в уже имеющемся расчете зарплаты, " +
						"документы попадающие в этот интервал будут пересчитываться по другому расчету! " +
						"Продолжить?", "Внимание!")) {
						return;
					}

					var previousParameter = GetPreviousParameter(node.WageParameter.StartDate);
					if(previousParameter != null) {
						previousParameter.EndDate = StartDate.Value.AddTicks(-1);
					}
					node.WageParameter.StartDate = StartDate.Value;
					WageParametersUpdated();
				},
				(node) => {
					if(node == null || !StartDate.HasValue) {
						return false;
					}
					var previousParameterByDate = GetPreviousParameter(StartDate.Value);
					var previousParameterBySelectedParameter = GetPreviousParameter(node.WageParameter.StartDate);

					bool noConflictWithEndDate = !node.WageParameter.EndDate.HasValue || node.WageParameter.EndDate.Value > StartDate;
					bool noConflictWithPreviousStartDate = (previousParameterByDate == null && previousParameterBySelectedParameter == null) || (previousParameterBySelectedParameter != null && previousParameterBySelectedParameter.StartDate < StartDate);

					return StartDate.HasValue && noConflictWithEndDate && noConflictWithPreviousStartDate;
				}
			);
			ChangeWageStartDateCommand.CanExecuteChangedWith(this, x => x.StartDate);
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

		public DelegateCommand<WageParameterNode> OpenWageParameterCommand { get; private set; }

		private void CreateOpenWageParameterCommand()
		{
			OpenWageParameterCommand = new DelegateCommand<WageParameterNode>(
				(node) => {
					WageParameterViewModel wageParameterViewModel = new WageParameterViewModel(node.WageParameter, UoW, CommonServices, navigationManager);
					tab.TabParent.AddTab(wageParameterViewModel, tab);
				},
				(node) => node != null
			);
		}

		#endregion OpenWageParameterCommand
	}
}
