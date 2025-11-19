using Autofac;
using DateTimeHelpers;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Report;
using QS.Services;
using QS.Tdi;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Services;
using Vodovoz.Settings.Contacts;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Contacts;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Counterparties
{
	public class CallTaskViewModel : EntityDialogViewModelBase<CallTask>, ISaveable, IHasChanges
	{
		private Action _openReportByCounterpartyLegacyCallback;
		private Action _openReportByDeliveryPointLegacyCallback;
		private bool _canCreateTask = false;
		private string _deliveryPointOrSelfDeliveryDebt;
		private string _bottleReserve;
		private string _oldComments;
		private string _counterpartyDebt;
		private readonly IBottlesRepository _bottlesRepository;
		private readonly ICallTaskRepository _callTaskRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ICommonServices _commonServices;

		private string _lastComment;
		private Action _createNewOrderLegacyCallback;
		private string _comment;

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public CallTaskViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IValidator validator,
			IEmployeeService employeeService,
			ViewModelEEVMBuilder<Employee> attachedEmployyeeViewModelEEVMBuilder,
			ViewModelEEVMBuilder<DeliveryPoint> deliveryPointViewModelEEVMBuilder,
			IBottlesRepository bottlesRepository,
			ICallTaskRepository callTaskRepository,
			IEmployeeRepository employeeRepository,
			ICommonServices commonServices,
			ILifetimeScope lifetimeScope)
			: base(uowBuilder, unitOfWorkFactory, navigation, validator)
		{
			if(attachedEmployyeeViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(attachedEmployyeeViewModelEEVMBuilder));
			}

			if(deliveryPointViewModelEEVMBuilder is null)
			{
				throw new ArgumentNullException(nameof(deliveryPointViewModelEEVMBuilder));
			}

			_bottlesRepository = bottlesRepository
				?? throw new ArgumentNullException(nameof(bottlesRepository));
			_callTaskRepository = callTaskRepository
				?? throw new ArgumentNullException(nameof(callTaskRepository));
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
			_commonServices = commonServices
				?? throw new ArgumentNullException(nameof(commonServices));
			LifetimeScope = lifetimeScope;

			ReportInfoFactory = lifetimeScope.Resolve<IReportInfoFactory>();

			if(UoW.IsNew)
			{
				Title = "Новая задача";
				Entity.CreationDate = DateTime.Now;
				Entity.Source = TaskSource.Handmade;
				Entity.TaskCreator = employeeService.GetEmployeeForCurrentUser(UoW);
				Entity.EndActivePeriod = DateTime.Now.AddDays(1);
			}
			else
			{
				Title = Entity.Counterparty?.Name;
			}

			Entity.PropertyChanged += OnEntityPropertyChanged;

			AttachedEmployeeViewModel = attachedEmployyeeViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, e => e.AssignedEmployee)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.RestrictCategory = EmployeeCategory.office;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			DeliveryPointViewModel = deliveryPointViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, e => e.DeliveryPoint)
				.UseViewModelJournalAndAutocompleter<DeliveryPointJournalViewModel, DeliveryPointJournalFilterViewModel>(filter =>
				{
					filter.Counterparty = Entity.Counterparty;
				})
				.UseViewModelDialog<DeliveryPointViewModel>()
				.Finish();

			DeliveryPointViewModel.IsEditable = CanChengeDeliveryPoint;

			CounterpartyPhonesViewModel =
				new PhonesViewModel(
					_commonServices,
					LifetimeScope.Resolve<IPhoneRepository>(),
					UoW,
					LifetimeScope.Resolve<IContactSettings>(),
					LifetimeScope.Resolve<IPhoneTypeSettings>(),
					LifetimeScope.Resolve<IExternalCounterpartyController>()
				)
				{
					ReadOnly = true
				};

			DeliveryPointPhonesViewModel = 
				new PhonesViewModel(
					_commonServices,
					LifetimeScope.Resolve<IPhoneRepository>(),
					UoW,
					LifetimeScope.Resolve<IContactSettings>(),
					LifetimeScope.Resolve<IPhoneTypeSettings>(),
					LifetimeScope.Resolve<IExternalCounterpartyController>()
				)
				{
					ReadOnly = true
				};

			CreateReportByCounterpartyCommand = new DelegateCommand(CreateReportByCounterparty, () => CanCreateReportByCounterparty);
			CreateReportByCounterpartyCommand.CanExecuteChangedWith(this, vm => vm.CanCreateReportByCounterparty);

			CreateReportByDeliveryPointCommand = new DelegateCommand(CreateReportByDeliveryPoint, () => CanCreateReportByDeliveryPoint);
			CreateReportByDeliveryPointCommand.CanExecuteChangedWith(this, vm => CanCreateReportByDeliveryPoint);

			AddCommentCommand = new DelegateCommand(AddComment);
			CancelLastCommentCommand = new DelegateCommand(CancelLastComment);

			CreateNewOrderCommand = new DelegateCommand(CreateNewOrder);

			CreateNewTaskCommand = new DelegateCommand(CreateNewTask, () => CanCreateNewTask);
			CreateNewTaskCommand.CanExecuteChangedWith(this, vm => vm.CanCreateNewTask);

			SaveCommand = new DelegateCommand(() => SaveAndClose());
			CloseCommand = new DelegateCommand(() => Close(true, CloseSource.Cancel));

			UpdateCounterpartyInformation();
			UpdateDeliveryPointInformation();
		}

		public IReportInfoFactory ReportInfoFactory { get; }

		public IEntityEntryViewModel AttachedEmployeeViewModel { get; }
		public IEntityEntryViewModel DeliveryPointViewModel { get; }

		public PhonesViewModel CounterpartyPhonesViewModel { get; }
		public PhonesViewModel DeliveryPointPhonesViewModel { get; }
		public ILifetimeScope LifetimeScope { get; }

		public DelegateCommand CreateReportByCounterpartyCommand { get; }
		public DelegateCommand CreateReportByDeliveryPointCommand { get; }
		public DelegateCommand AddCommentCommand { get; }
		public DelegateCommand CancelLastCommentCommand { get; }

		public DelegateCommand CreateNewOrderCommand { get; }
		public DelegateCommand CreateNewTaskCommand { get; }

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CloseCommand { get; }

		public bool CanCreateTask
		{
			get => _canCreateTask;
			private set => SetField(ref _canCreateTask, value);
		}

		public string DeliveryPointOrSelfDeliveryDebt
		{
			get => _deliveryPointOrSelfDeliveryDebt;
			private set => SetField(ref _deliveryPointOrSelfDeliveryDebt, value);
		}

		public string BottleReserve
		{
			get => _bottleReserve;
			private set => SetField(ref _bottleReserve, value);
		}

		public string OldComments
		{
			get => _oldComments;
			private set => SetField(ref _oldComments, value);
		}

		public string CounterpartyDebt
		{
			get => _counterpartyDebt;
			private set => SetField(ref _counterpartyDebt, value);
		}

		public string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		public string TaskCreatorString => $"Создатель : {Entity.TaskCreator?.ShortName}";

		public string TaskCompletedAtString => $"Задача выполнена {Entity.CompleteDate?.ToString("dd / MM / yyyy  HH:mm")}";

		public bool CanCreateReportByCounterparty => Entity.Counterparty != null;

		public bool CanCreateReportByDeliveryPoint => Entity.DeliveryPoint != null;

		public bool CanChengeDeliveryPoint => Entity.Counterparty != null;

		public bool CanCreateNewTask => Entity.Id != 0;

		public void SetCounterpartyById(int counterpartyId)
		{
			Entity.Counterparty = UoW.GetById<Counterparty>(counterpartyId);
		}

		public void SetDeliveryPointById(int deliveryPointId)
		{
			Entity.DeliveryPoint = UoW.GetById<DeliveryPoint>(deliveryPointId);
		}

		[Obsolete("Убрать при обновлении")]
		public void SetCreateReportByCounterpartyLegacyCallback(Action action)
		{
			_openReportByCounterpartyLegacyCallback = action;
		}

		[Obsolete("Убрать при обновлении")]
		public void SetCreateReportByDeliveryPointLegacyCallback(Action action)
		{
			_openReportByDeliveryPointLegacyCallback = action;
		}

		[Obsolete("Убрать при обновлении")]
		public void SetCreateNewOrderLegacyCallback(Action action)
		{
			_createNewOrderLegacyCallback = action;
		}

		private void AddComment()
		{
			if(string.IsNullOrEmpty(Comment))
			{
				return;
			}

			Entity.AddComment(UoW, Comment, out _lastComment, _employeeRepository);
			Comment = string.Empty;
		}

		private void CancelLastComment()
		{
			if(string.IsNullOrEmpty(_lastComment))
			{
				return;
			}

			var lastIndexOfComment = Entity.Comment.LastIndexOf(_lastComment);

			Entity.Comment = Entity.Comment.Substring(0, lastIndexOfComment);

			_lastComment = string.Empty;
		}

		private void CreateReportByCounterparty()
		{
			_openReportByCounterpartyLegacyCallback.Invoke();
		}

		private void CreateReportByDeliveryPoint()
		{
			_openReportByDeliveryPointLegacyCallback.Invoke();
		}

		private void CreateNewOrder()
		{
			if(Entity.Counterparty is null)
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Для оформления заказа требуется заполненный контрагент в задаче",
					"Ошибка");
				return;
			}

			_createNewOrderLegacyCallback.Invoke();
		}

		private void CreateNewTask()
		{
			NavigationManager.OpenViewModel<CallTaskViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate(), OpenPageOptions.None, vm =>
			{
				vm.Entity.DeliveryPoint =
					Entity.DeliveryPoint != null
					? vm.UoW.GetById<DeliveryPoint>(Entity.DeliveryPoint.Id)
					: null;

				vm.Entity.Counterparty = vm.UoW.GetById<Counterparty>(Entity.Counterparty.Id);

				vm.Entity.AssignedEmployee =
					Entity.AssignedEmployee != null
					? vm.UoW.GetById<Employee>(Entity.AssignedEmployee.Id)
					: null;

				vm.Entity.EndActivePeriod = DateTime.Now.LatestDayTime();

				vm.UpdateCounterpartyInformation();
				vm.UpdateDeliveryPointInformation();
			});
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Id))
			{
				OnPropertyChanged(nameof(CanCreateNewTask));
			}

			if(e.PropertyName == nameof(Entity.Counterparty))
			{
				UpdateCounterpartyInformation();

				if(Entity.Counterparty != null && Entity.Counterparty.DeliveryPoints.Count == 1)
				{
					Entity.DeliveryPoint =
						Entity.Counterparty.DeliveryPoints.First();
				}

				if(Entity.Counterparty is null || !Entity.Counterparty.DeliveryPoints.Contains(Entity.DeliveryPoint))
				{
					Entity.DeliveryPoint = null;
				}

				OnPropertyChanged(nameof(CanCreateReportByCounterparty));
				DeliveryPointViewModel.IsEditable = CanChengeDeliveryPoint;
				return;
			}

			if(e.PropertyName == nameof(Entity.DeliveryPoint))
			{
				UpdateDeliveryPointInformation();

				OnPropertyChanged(nameof(CanCreateReportByDeliveryPoint));
				return;
			}

			if(e.PropertyName == nameof(Entity.TaskCreator))
			{
				OnPropertyChanged(nameof(TaskCreatorString));

				return;
			}

			if(e.PropertyName == nameof(Entity.CompleteDate))
			{
				OnPropertyChanged(nameof(TaskCompletedAtString));

				return;
			}
		}

		private void UpdateCounterpartyInformation()
		{
			if(Entity.Counterparty != null)
			{
				CounterpartyDebt = _bottlesRepository.GetBottlesDebtAtCounterparty(UoW, Entity.Counterparty).ToString();

				CounterpartyPhonesViewModel.PhonesList = Entity.Counterparty.ObservablePhones;
			}
			else
			{
				CounterpartyDebt = string.Empty;
				CounterpartyPhonesViewModel.PhonesList = null;
			}
		}

		private void UpdateDeliveryPointInformation()
		{
			if(Entity.DeliveryPoint != null)
			{
				DeliveryPointOrSelfDeliveryDebt = _bottlesRepository
					.GetBottlesDebtAtDeliveryPoint(UoW, Entity.DeliveryPoint)
					.ToString();

				BottleReserve = Entity.DeliveryPoint.BottleReserv.ToString();

				DeliveryPointPhonesViewModel.PhonesList =
					Entity.DeliveryPoint.ObservablePhones;

				OldComments = _callTaskRepository.GetCommentsByDeliveryPoint(UoW, Entity.DeliveryPoint, Entity);
			}
			else
			{
				if(Entity.Counterparty != null)
				{
					DeliveryPointOrSelfDeliveryDebt = _bottlesRepository.GetBottleDebtBySelfDelivery(UoW, Entity.Counterparty).ToString();
				}
				else
				{
					DeliveryPointOrSelfDeliveryDebt = string.Empty;
				}

				BottleReserve = string.Empty;
				OldComments = Entity.Comment;
				DeliveryPointPhonesViewModel.PhonesList = null;
			}
		}

		bool ISaveable.Save()
		{
			base.Save();
			EntitySaved?.Invoke(this, new EntitySavedEventArgs(Entity));
			return true;
		}

		void ISaveable.SaveAndClose()
		{
			SaveAndClose();
			EntitySaved?.Invoke(this, new EntitySavedEventArgs(Entity));
		}
	}
}
