using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Tdi;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using QS.ViewModels.Control.EEVM;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.ViewModels.ViewModels.Employees;
using QS.Dialog;

namespace Vodovoz.ViewModels.Counterparties
{
	public class CallTaskMassEditViewModel : DialogViewModelBase, IDisposable, IHasChanges, ISaveable
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IInteractiveService _interactiveService;
		private IUnitOfWork _unitOfWork;
		private Employee _assignedEmployee;
		private CallTaskStatus? _callTaskStatus;
		private DateTime? _endActivePeriod;
		private bool? _isTaskComplete;

		public CallTaskMassEditViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IPermissionService permissionService,
			ViewModelEEVMBuilder<Employee> attachedEmployyeeViewModelEEVMBuilder,
			IUserService userService,
			IInteractiveService interactiveService,
			INavigationManager navigation)
			: base(navigation)
		{
			if(permissionService is null)
			{
				throw new ArgumentNullException(nameof(permissionService));
			}

			if(!permissionService
					.ValidateUserPermission(
						typeof(CallTask),
						userService.GetCurrentUser().Id).CanUpdate)
			{
				throw new AbortCreatingPageException("У вас нет прав для доступа к этому дмалогу", "Ошибка доступа!");
			}

			Title = "Массовое редактирование задач на обзвон";

			_unitOfWorkFactory = unitOfWorkFactory
				?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_interactiveService = interactiveService
				?? throw new ArgumentNullException(nameof(interactiveService));
			_unitOfWork = _unitOfWorkFactory.CreateWithoutRoot(Title);

			Tasks = new GenericObservableList<CallTask>();

			AttachedEmployeeViewModel = attachedEmployyeeViewModelEEVMBuilder
				.SetUnitOfWork(_unitOfWork)
				.SetViewModel(this)
				.ForProperty(this, e => e.AssignedEmployee)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.RestrictCategory = EmployeeCategory.office;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			SaveCommand = new DelegateCommand(() => SaveAndClose(), () => HasChanges);
			SaveCommand.CanExecuteChangedWith(this, x => x.HasChanges);
			CloseCommand = new DelegateCommand(() => Close(HasChanges, CloseSource.Cancel));

			ResetChangeAssignedEmployeeCommand = new DelegateCommand(
				ResetChangeAssignedEmployee,
				() => NeedToChangeAssignedEmployee);
			ResetChangeAssignedEmployeeCommand.CanExecuteChangedWith(this, x => x.NeedToChangeAssignedEmployee);

			ResetCallTaskStatusCommand = new DelegateCommand(
				ResetCallTaskStatus,
				() => NeedToChangeCallTaskStatus);
			ResetCallTaskStatusCommand.CanExecuteChangedWith(this, x => x.NeedToChangeCallTaskStatus);

			ResetIsTaskCompleteCommand = new DelegateCommand(
				ResetIsTaskComplete,
				() => NeedToChangeIsTaskComplete);
			ResetIsTaskCompleteCommand.CanExecuteChangedWith(this, x => x.NeedToChangeIsTaskComplete);

			ResetEndActivePeriodCommand = new DelegateCommand(
				ResetEndActivePeriod,
				() => NeedToChangeEndActivePeriod);
			ResetEndActivePeriodCommand.CanExecuteChangedWith(this, x => x.NeedToChangeEndActivePeriod);

			ShowInformationCommand = new DelegateCommand(ShowInformation);
		}

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public IEntityEntryViewModel AttachedEmployeeViewModel { get; }

		public GenericObservableList<CallTask> Tasks { get; }

		[PropertyChangedAlso(nameof(HasChanges),
			nameof(NeedToChangeAssignedEmployee))]
		public Employee AssignedEmployee
		{
			get => _assignedEmployee;
			set => SetField(ref _assignedEmployee, value);
		}

		public bool NeedToChangeAssignedEmployee => AssignedEmployee != null;

		[PropertyChangedAlso(nameof(HasChanges),
			nameof(NeedToChangeCallTaskStatus))]
		public CallTaskStatus? CallTaskStatus
		{
			get => _callTaskStatus;
			set => SetField(ref _callTaskStatus, value);
		}

		public bool NeedToChangeCallTaskStatus => CallTaskStatus != null;

		[PropertyChangedAlso(nameof(HasChanges),
			nameof(NeedToChangeIsTaskComplete))]
		public bool? IsTaskComplete
		{
			get => _isTaskComplete;
			set => SetField(ref _isTaskComplete, value);
		}

		public bool NeedToChangeIsTaskComplete => IsTaskComplete != null;

		[PropertyChangedAlso(nameof(HasChanges),
			nameof(NeedToChangeEndActivePeriod))]
		public DateTime? EndActivePeriod
		{
			get => _endActivePeriod;
			set => SetField(ref _endActivePeriod, value);
		}

		public bool NeedToChangeEndActivePeriod => EndActivePeriod != null;

		public bool HasChanges =>
			NeedToChangeAssignedEmployee
			|| NeedToChangeCallTaskStatus
			|| NeedToChangeIsTaskComplete
			|| NeedToChangeEndActivePeriod;

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CloseCommand { get; }
		public DelegateCommand ResetChangeAssignedEmployeeCommand { get; }
		public DelegateCommand ResetCallTaskStatusCommand { get; }
		public DelegateCommand ResetIsTaskCompleteCommand { get; }
		public DelegateCommand ResetEndActivePeriodCommand { get; }
		public DelegateCommand ShowInformationCommand { get; }

		public void AddTasks(IEnumerable<int> ids)
		{
			var existingTasksIds = Tasks.Select(t => t.Id).ToArray();

			var tasksToAdd = _unitOfWork.Session.Query<CallTask>()
				.Where(x => ids.Contains(x.Id)
					&& !existingTasksIds.Contains(x.Id))
				.ToList();

			foreach(var task in tasksToAdd)
			{
				Tasks.Add(task);
			}
		}

		private void ResetChangeAssignedEmployee()
		{
			AssignedEmployee = null;
		}

		private void ResetCallTaskStatus()
		{
			CallTaskStatus = null;
		}

		private void ResetIsTaskComplete()
		{
			IsTaskComplete = false;
			IsTaskComplete = null;
		}

		private void ResetEndActivePeriod()
		{
			EndActivePeriod = null;
		}

		private void ShowInformation()
		{
			_interactiveService.ShowMessage(
				ImportanceLevel.Info,
				"У каждого элемента для изменения в задаче на обзвон слева есть кнопка с иконкой восклицательного знака\n" +
				"Если кнопка не активна - изменение в данном поле не будет применено\n" +
				"Если кнопка активна - данное изменение будет применено ко всем выбранным задачам на обзвон\n" +
				"\n" +
				"Изменения вносятся после нажатия кнопки сохранить",
				"Информация");
		}

		public bool Save()
		{
			if(!HasChanges)
			{
				return true;
			}

			foreach(var task in Tasks)
			{
				if(NeedToChangeAssignedEmployee)
				{
					task.AssignedEmployee = AssignedEmployee;
				}

				if(NeedToChangeCallTaskStatus)
				{
					task.TaskState = CallTaskStatus.Value;
				}

				if(NeedToChangeIsTaskComplete)
				{
					task.IsTaskComplete = IsTaskComplete.Value;
				}

				if(NeedToChangeEndActivePeriod)
				{
					task.EndActivePeriod = EndActivePeriod.Value;
				}

				_unitOfWork.Save(task);
			}

			_unitOfWork.Commit();
			return true;
		}

		public void SaveAndClose()
		{
			Save();
			Close(false, CloseSource.Save);
		}

		public void Dispose()
		{
			_unitOfWork?.Dispose();
		}
	}
}
