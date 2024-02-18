﻿using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QSReport;
using System;
using System.Linq;
using Vodovoz.Domain.BusinessTasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Comments;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Models;
using Vodovoz.Settings.Contacts;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Contacts;

namespace Vodovoz.ViewModels.BusinessTasks
{
	public class ClientTaskViewModel : EntityTabViewModelBase<ClientTask>
	{
		private string oldComments;
		public string OldComments {
			get => oldComments;
			set => SetField(ref oldComments, value);
		}

		private string debtByAddress;
		public string DebtByAddress {
			get => debtByAddress;
			set => SetField(ref debtByAddress, value);
		}

		private string debtByClient;
		public string DebtByClient {
			get => debtByClient;
			set => SetField(ref debtByClient, value);
		}

		private string bottleReserve;
		public string BottleReserve {
			get => bottleReserve;
			set => SetField(ref bottleReserve, value);
		}

		public bool TaskButtonVisibility { get; set; } = true;

		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; private set; }
		public IEntityAutocompleteSelectorFactory DeliveryPointFactory { get; private set; }

		public PhonesViewModel ClientPhonesVM { get; private set; }
		public PhonesViewModel DeliveryPointPhonesVM { get; private set; }

		public readonly IEmployeeRepository employeeRepository;
		public readonly IBottlesRepository bottleRepository;
		public readonly IPhoneRepository phoneRepository;
		private readonly IOrganizationProvider organizationProvider;
		private readonly ICounterpartyContractRepository counterpartyContractRepository;
		private readonly ICounterpartyContractFactory counterpartyContractFactory;
		private readonly RoboatsJournalsFactory _roboAtsCounterpartyJournalFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IContactSettings _contactsParameters;

		public ClientTaskViewModel(
			IEmployeeRepository employeeRepository,
			IBottlesRepository bottleRepository,
			IPhoneRepository phoneRepository,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOrganizationProvider organizationProvider,
			ICounterpartyContractRepository counterpartyContractRepository,
			ICounterpartyContractFactory counterpartyContractFactory,
			IContactSettings contactsParameters,
			ICommonServices commonServices,
			RoboatsJournalsFactory roboAtsCounterpartyJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			ILifetimeScope lifetimeScope)
			: base (uowBuilder, unitOfWorkFactory, commonServices)
		{
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			this.bottleRepository = bottleRepository ?? throw new ArgumentNullException(nameof(bottleRepository));
			this.phoneRepository = phoneRepository ?? throw new ArgumentNullException(nameof(phoneRepository));
			this.organizationProvider = organizationProvider ?? throw new ArgumentNullException(nameof(organizationProvider));
			this.counterpartyContractRepository = counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
			this.counterpartyContractFactory = counterpartyContractFactory ?? throw new ArgumentNullException(nameof(counterpartyContractFactory));
			_contactsParameters = contactsParameters ?? throw new ArgumentNullException(nameof(contactsParameters));
			_roboAtsCounterpartyJournalFactory = roboAtsCounterpartyJournalFactory ?? throw new ArgumentNullException(nameof(roboAtsCounterpartyJournalFactory));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_lifetimeScope = lifetimeScope;
			if(uowBuilder.IsNewEntity) {
				TabName = "Новая задача";
				Entity.CreationDate = DateTime.Now;
				Entity.Source = Domain.BusinessTasks.TaskSource.Handmade;
				Entity.TaskCreator = employeeRepository.GetEmployeeForCurrentUser(UoW);
				Entity.EndActivePeriod = DateTime.Now.AddDays(1);
				TaskButtonVisibility = false;
			} else {
				TabName = Entity.Counterparty?.Name;
			}

			CounterpartySelectorFactory = _counterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory(lifetimeScope);

			Initialize();
			CreateCommands();
			UpdateAddressFields();
		}

		public ClientTaskViewModel(
			IEmployeeRepository employeeRepository,
			IBottlesRepository bottleRepository,
			IPhoneRepository phoneRepository,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IOrganizationProvider organizationProvider,
			ICounterpartyContractRepository counterpartyContractRepository,
			ICounterpartyContractFactory counterpartyContractFactory,
			RoboatsJournalsFactory roboAtsCounterpartyJournalFactory,
			IContactSettings contactsParameters,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			ILifetimeScope lifetimeScope,
			int counterpartyId,
			int deliveryPointId)
			: this(employeeRepository,
					bottleRepository,
					phoneRepository,
					uowBuilder,
					unitOfWorkFactory,
					organizationProvider,
					counterpartyContractRepository,
					counterpartyContractFactory,
					contactsParameters,
					commonServices,
					roboAtsCounterpartyJournalFactory,
					counterpartyJournalFactory,
					lifetimeScope)
		{
			Entity.Counterparty = UoW.GetById<Counterparty>(counterpartyId);
			Entity.DeliveryPoint = UoW.GetById<DeliveryPoint>(deliveryPointId);
		}

		private void Initialize()
		{
			ClientPhonesVM = CreatePhonesViewModel();
			DeliveryPointPhonesVM = CreatePhonesViewModel();

			EmployeeSelectorFactory = new EmployeeJournalFactory(NavigationManager).CreateWorkingOfficeEmployeeAutocompleteSelectorFactory();

			DeliveryPointFactory = CreateDeliveryPointFactory();
		}

		private IEntityAutocompleteSelectorFactory CreateDeliveryPointFactory()
		{
			var dpFilter = new DeliveryPointJournalFilterViewModel{Counterparty = Entity.Counterparty, HidenByDefault = true};
			var deliveryPointFactory = _lifetimeScope.Resolve<IDeliveryPointJournalFactory>();
			deliveryPointFactory.SetDeliveryPointJournalFilterViewModel(dpFilter);

			return deliveryPointFactory
				.CreateDeliveryPointByClientAutocompleteSelectorFactory();
		}

		private PhonesViewModel CreatePhonesViewModel()
		{
			var phoneTypeSettings = ScopeProvider.Scope.Resolve<IPhoneTypeSettings>();
			return new PhonesViewModel(phoneTypeSettings, phoneRepository, UoW, _contactsParameters, _roboAtsCounterpartyJournalFactory, CommonServices) {
				ReadOnly = true
			};
		}

		private void CreateCommands()
		{
			CreateSaveCommand();
			CreateCancelCommand();
			CreateOrderCommand();
			CreateTaskCommand();
			CreateReportByDPCommand();
			CreateReportByClientcommand();
		}

		public DelegateCommand SaveCommand { get; private set; }

		private void CreateSaveCommand()
		{
			SaveCommand = new DelegateCommand(
				() => Save(true),
				() => true
			);
		}

		public DelegateCommand CancelCommand { get; private set; }

		private void CreateCancelCommand()
		{
			CancelCommand = new DelegateCommand(
				() => Close(true, QS.Navigation.CloseSource.Cancel),
				() => true
			);
		}

		public DelegateCommand CreateNewOrderCommand { get; private set; }

		private void CreateOrderCommand()
		{
			CreateNewOrderCommand = new DelegateCommand(
				() => {

					OrderDlg orderDlg = new OrderDlg();
					orderDlg.Entity.Client = orderDlg.UoW.GetById<Counterparty>(Entity.Counterparty.Id);
					orderDlg.Entity.UpdateClientDefaultParam(UoW, counterpartyContractRepository, organizationProvider, counterpartyContractFactory);
					orderDlg.Entity.DeliveryPoint = orderDlg.UoW.GetById<DeliveryPoint>(Entity.DeliveryPoint.Id);

					orderDlg.CallTaskWorker.TaskCreationInteractive = new GtkTaskCreationInteractive();
					TabParent.AddTab(orderDlg, this);
				},
				() => Entity.DeliveryPoint != null
			);
		}

		public DelegateCommand CreateNewTaskCommand { get; private set; }

		private void CreateTaskCommand()
		{
			CreateNewTaskCommand = new DelegateCommand(
				() => {
					//ClientTaskViewModel newTask = new ClientTaskViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, CommonServices);
					//CallTaskSingletonFactory.GetInstance().CopyTask(UoW, employeeRepository, Entity, newTask.Entity);
					//newTask.UpdateAddressFields();
					//TabParent.AddTab(newTask, this);
				},
				() => Entity.DeliveryPoint != null
			);
		}

		public DelegateCommand OpenReportByDPCommand { get; private set; }

		private void CreateReportByDPCommand()
		{
			OpenReportByDPCommand = new DelegateCommand(
				() => { TabParent.AddTab(new ReportViewDlg(Entity.CreateReportInfoByDeliveryPoint()), this); },
				() => true
			);
		}

		public DelegateCommand OpenReportByClientCommand { get; private set; }

		private void CreateReportByClientcommand()
		{
			OpenReportByClientCommand = new DelegateCommand(
				() => { TabParent.AddTab(new ReportViewDlg(Entity.CreateReportInfoByClient()), this); },
				() => true
			);
		}

		protected override bool BeforeSave()
		{
			if(Entity.ObservableComments.Any()) {

				foreach(DocumentComment docComment in Entity.ObservableComments) {
					UoW.Save(docComment);
				}
			}
			return base.BeforeSave();
		}

		public void OnDeliveryPointVMEntryChangedByUser(object sender, EventArgs e)
		{
			if(Entity.DeliveryPoint != null && Entity.Counterparty == null)
				Entity.Counterparty = Entity.DeliveryPoint.Counterparty;

			UpdateAddressFields();
		}

		public void OnCounterpartyViewModelEntryChangedByUser(object sender, EventArgs e)
		{
			DeliveryPointFactory = CreateDeliveryPointFactory();
			if(Entity.Counterparty != null) {
				if(Entity.Counterparty.Id != Entity.DeliveryPoint?.Counterparty.Id) {
					if(Entity.Counterparty.DeliveryPoints.Count == 1)
						Entity.DeliveryPoint = Entity.Counterparty.DeliveryPoints[0];
					else
						Entity.DeliveryPoint = null;
				}
			}
			UpdateAddressFields();
		}

		private void UpdateAddressFields()
		{
			if(Entity.DeliveryPoint != null) {
				DebtByAddress = bottleRepository.GetBottlesDebtAtDeliveryPoint(UoW, Entity.DeliveryPoint).ToString();
				BottleReserve = Entity.DeliveryPoint.BottleReserv.ToString();
				DeliveryPointPhonesVM.PhonesList = Entity.DeliveryPoint.ObservablePhones;
				//OldComments = callTaskRepository.GetCommentsByDeliveryPoint(UoW, Entity.DeliveryPoint, Entity);
			} else {
				DebtByAddress = string.Empty;
				BottleReserve = string.Empty;
				DeliveryPointPhonesVM.PhonesList = null;

				if(Entity.ObservableComments.Any()) {

					foreach(DocumentComment docComment in Entity.ObservableComments) {
						OldComments += docComment;
					}
				}
			}

			UpdateClienInfoFields();
		}

		private void UpdateClienInfoFields()
		{
			if(Entity.Counterparty != null) {
				DebtByClient = bottleRepository.GetBottlesDebtAtCounterparty(UoW, Entity.Counterparty).ToString();
				ClientPhonesVM.PhonesList = Entity.Counterparty?.ObservablePhones;
				if(Entity.DeliveryPoint == null)
				{
					DebtByAddress = bottleRepository.GetBottleDebtBySelfDelivery(UoW, Entity.Counterparty).ToString();
				}
			} else {
				DebtByClient = string.Empty;
				ClientPhonesVM.PhonesList = null;
			}
		}
	}
}
