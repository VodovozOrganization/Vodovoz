﻿using System;
using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Complaints
{
	public class CreateComplaintViewModel : EntityTabViewModelBase<Complaint>
	{
		private readonly IEntityAutocompleteSelectorFactory _employeeSelectorFactory;
		private readonly IList<ComplaintKind> _complaintKinds;
		private readonly IEmployeeService _employeeService;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IUserRepository _userRepository;
		private readonly IFileDialogService _fileDialogService;
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider;
        private IList<ComplaintObject> _complaintObjectSource;
        private ComplaintObject _complaintObject;
        private DelegateCommand _changeDeliveryPointCommand;

		public CreateComplaintViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			IUserRepository userRepository,
            IFileDialogService fileDialogService,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			string phone = null) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_employeeSelectorFactory = employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			CounterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			DeliveryPointJournalFactory = deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			_subdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));

			if(orderSelectorFactory == null)
			{
				throw new ArgumentNullException(nameof(orderSelectorFactory));
			}

			Entity.ComplaintType = ComplaintType.Client;
			Entity.SetStatus(ComplaintStatuses.Checking);
			ConfigureEntityPropertyChanges();
			Entity.Phone = phone;

			_complaintKinds = complaintKindSource = UoW.GetAll<ComplaintKind>().Where(k => !k.IsArchive).ToList();

			UserHasOnlyAccessToWarehouseAndComplaints =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
			    && !ServicesConfig.CommonServices.UserService.GetCurrentUser(UoW).IsAdmin;

			TabName = "Новая клиентская рекламация";
			
			InitializeOrderAutocompleteSelectorFactory(orderSelectorFactory);
		}

		public CreateComplaintViewModel(Counterparty client,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			IUserRepository userRepository,
            IFileDialogService filePickerService,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			string phone = null) : this(uowBuilder, unitOfWorkFactory, employeeService,
			subdivisionRepository, commonServices, userRepository, filePickerService, orderSelectorFactory, employeeJournalFactory,
			counterpartyJournalFactory, deliveryPointJournalFactory,subdivisionParametersProvider, phone)
		{
			var currentClient = UoW.GetById<Counterparty>(client.Id);
			Entity.Counterparty = currentClient;
			Entity.Phone = phone;
		}

		public CreateComplaintViewModel(Order order,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			IUserRepository userRepository,
            IFileDialogService filePickerService,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			string phone = null) : this(uowBuilder, unitOfWorkFactory, employeeService, subdivisionRepository,
			commonServices, userRepository, filePickerService, orderSelectorFactory, employeeJournalFactory, counterpartyJournalFactory,
			deliveryPointJournalFactory, subdivisionParametersProvider, phone)
		{
			var currentOrder = UoW.GetById<Order>(order.Id);
			Entity.Order = currentOrder;
			Entity.Counterparty = currentOrder.Client;
			Entity.Phone = phone;
		}
		
		private void InitializeOrderAutocompleteSelectorFactory(IOrderSelectorFactory orderSelectorFactory)
		{
			var orderFilter =
				new OrderJournalFilterViewModel(CounterpartyJournalFactory, DeliveryPointJournalFactory, EmployeeJournalFactory);
			
			if(Entity.Counterparty != null)
			{
				orderFilter.RestrictCounterparty = Entity.Counterparty;
			}
			
			OrderAutocompleteSelectorFactory =
				(orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory)))
				.CreateOrderAutocompleteSelectorFactory(orderFilter);
		}

		private Employee currentEmployee;
		public Employee CurrentEmployee {
			get {
				if(currentEmployee == null) {
					currentEmployee = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}
				return currentEmployee;
			}
		}

        private ComplaintFilesViewModel filesViewModel;
        public ComplaintFilesViewModel FilesViewModel
        {
            get
            {
                if (filesViewModel == null)
                {
                    filesViewModel = new ComplaintFilesViewModel(Entity, UoW, _fileDialogService, CommonServices, _userRepository);
                }
                return filesViewModel;
            }
        }

        //так как диалог только для создания рекламации
        public bool CanEdit => PermissionResult.CanCreate;

		public bool CanSelectDeliveryPoint => Entity.Counterparty != null;

		private List<ComplaintSource> complaintSources;

		public IEnumerable<ComplaintSource> ComplaintSources {
			get {
				if(complaintSources == null) {
					complaintSources = UoW.GetAll<ComplaintSource>().ToList();
				}
				return complaintSources;
			}
		}

		IList<ComplaintKind> complaintKindSource;
		public IList<ComplaintKind> ComplaintKindSource {
			get => complaintKindSource;
			set => SetField(ref complaintKindSource, value);
		}

		public virtual ComplaintObject ComplaintObject
		{
			get => _complaintObject;
			set
			{
				if(SetField(ref _complaintObject, value))
				{
					ComplaintKindSource = value == null ? _complaintKinds : _complaintKinds.Where(x => x.ComplaintObject == value).ToList();
				}
			}
		}

		public IEnumerable<ComplaintObject> ComplaintObjectSource =>
			_complaintObjectSource ?? (_complaintObjectSource = UoW.GetAll<ComplaintObject>().Where(x => !x.IsArchive).ToList());

		private GuiltyItemsViewModel guiltyItemsViewModel;
		public GuiltyItemsViewModel GuiltyItemsViewModel {
			get {
				if(guiltyItemsViewModel == null) {
					guiltyItemsViewModel = new GuiltyItemsViewModel(Entity, UoW, CommonServices, _subdivisionRepository, _employeeSelectorFactory, _subdivisionParametersProvider);
				}

				return guiltyItemsViewModel;
			}
		}

		protected override void BeforeValidation()
		{
			if(UoW.IsNew) {
				Entity.CreatedBy = CurrentEmployee;
				Entity.CreationDate = DateTime.Now;
				Entity.PlannedCompletionDate = DateTime.Today;
			}
			Entity.ChangedBy = CurrentEmployee;
			Entity.ChangedDate = DateTime.Now;

			base.BeforeValidation();
		}

		void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(
				e => e.Counterparty,
				() => CanSelectDeliveryPoint
			);
		}

        public void CheckAndSave()
        {
            if (!HasСounterpartyDuplicateToday() ||
                CommonServices.InteractiveService.Question("Рекламация с данным контрагентом уже создавалась сегодня, создать ещё одну?"))
            {
                SaveAndClose();
            }
        }

        private bool HasСounterpartyDuplicateToday()
        {
	        if(Entity.Counterparty == null) {
		        return false;
	        }
	        return UoW.Session.QueryOver<Complaint>()
		        .Where(i => i.Counterparty.Id == Entity.Counterparty.Id)
		        .And(i => i.CreationDate >= DateTime.Now.AddDays(-1))
		        .RowCount() > 0;
        }

        #region ChangeDeliveryPointCommand

        public DelegateCommand ChangeDeliveryPointCommand => _changeDeliveryPointCommand ?? (_changeDeliveryPointCommand =
	        new DelegateCommand(() =>
		        {
			        if(Entity.Order?.DeliveryPoint != null)
			        {
				        Entity.DeliveryPoint = Entity.Order.DeliveryPoint;
			        }
		        },
		        () => true
	        ));

        #endregion ChangeDeliveryPointCommand

		public ICounterpartyJournalFactory CounterpartyJournalFactory { get; }
		public IEntityAutocompleteSelectorFactory OrderAutocompleteSelectorFactory { get; private set; }
		public bool UserHasOnlyAccessToWarehouseAndComplaints { get; }
		private IEmployeeJournalFactory EmployeeJournalFactory { get; }
		private IDeliveryPointJournalFactory DeliveryPointJournalFactory { get; }
	}
}
