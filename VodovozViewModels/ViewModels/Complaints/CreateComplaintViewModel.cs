using System;
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
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Infrastructure.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Complaints
{
	public class CreateComplaintViewModel : EntityTabViewModelBase<Complaint>
	{
		private readonly IEntityAutocompleteSelectorFactory _employeeSelectorFactory;
        private IList<ComplaintObject> _complaintObjectSource;
        private ComplaintObject _complaintObject;
        private readonly IList<ComplaintKind> _complaintKinds;
        private DelegateCommand _changeDeliveryPointCommand;

		public IEmployeeService EmployeeService { get; }
		public ISubdivisionRepository SubdivisionRepository { get; }
		public INomenclatureRepository NomenclatureRepository { get; }
		public IUserRepository UserRepository { get; }
		public IFileDialogService FileDialogService { get; }
		public bool UserHasOnlyAccessToWarehouseAndComplaints { get; }

		public CreateComplaintViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository,
            IFileDialogService fileDialogService,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IGtkTabsOpener gtkDialogsOpener,
			IUndeliveredOrdersJournalOpener undeliveredOrdersJournalOpener,
			INomenclatureJournalFactory nomenclatureSelector,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			string phone = null
		) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
            EmployeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			SubdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			NomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			FileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			OrderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_employeeSelectorFactory = employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			CounterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			DeliveryPointJournalFactory = deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			SubdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			GtkDialogsOpener = gtkDialogsOpener ?? throw new ArgumentNullException(nameof(gtkDialogsOpener));
			UndeliveredOrdersJournalOpener =
				undeliveredOrdersJournalOpener ?? throw new ArgumentNullException(nameof(undeliveredOrdersJournalOpener));
			NomenclatureSelector = nomenclatureSelector ?? throw new ArgumentNullException(nameof(nomenclatureSelector));
			UndeliveredOrdersRepository =
				undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));

			Entity.ComplaintType = ComplaintType.Client;
			Entity.SetStatus(ComplaintStatuses.Checking);
			ConfigureEntityPropertyChanges();
			Entity.Phone = phone;

			_complaintKinds = complaintKindSource = UoW.GetAll<ComplaintKind>().Where(k => !k.IsArchive).ToList();

			UserHasOnlyAccessToWarehouseAndComplaints =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
			    && !ServicesConfig.CommonServices.UserService.GetCurrentUser(UoW).IsAdmin;

			TabName = "Новая клиентская рекламация";
		}

		public CreateComplaintViewModel(Counterparty client,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository,
            IFileDialogService filePickerService,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IGtkTabsOpener gtkDialogsOpener,
			IUndeliveredOrdersJournalOpener undeliveredOrdersJournalOpener,
			INomenclatureJournalFactory nomenclatureSelector,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			string phone = null) : this(uowBuilder, unitOfWorkFactory, employeeService,
			subdivisionRepository, commonServices, nomenclatureRepository, userRepository, filePickerService,
			orderSelectorFactory, employeeJournalFactory, counterpartyJournalFactory, deliveryPointJournalFactory, subdivisionJournalFactory,
			gtkDialogsOpener, undeliveredOrdersJournalOpener, nomenclatureSelector, undeliveredOrdersRepository,
			phone)
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
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository,
            IFileDialogService filePickerService,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IGtkTabsOpener gtkDialogsOpener,
			IUndeliveredOrdersJournalOpener undeliveredOrdersJournalOpener,
			INomenclatureJournalFactory nomenclatureSelector,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			string phone = null) : this(uowBuilder, unitOfWorkFactory, employeeService, subdivisionRepository,
			commonServices, nomenclatureRepository, userRepository, filePickerService, orderSelectorFactory,
			employeeJournalFactory, counterpartyJournalFactory, deliveryPointJournalFactory, subdivisionJournalFactory, gtkDialogsOpener,
			undeliveredOrdersJournalOpener, nomenclatureSelector, undeliveredOrdersRepository, phone)
		{
			var currentOrder = UoW.GetById<Order>(order.Id);
			Entity.Order = currentOrder;
			Entity.Counterparty = currentOrder.Client;
			Entity.Phone = phone;
		}

		private Employee currentEmployee;
		public Employee CurrentEmployee {
			get {
				if(currentEmployee == null) {
					currentEmployee = EmployeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
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
                    filesViewModel = new ComplaintFilesViewModel(Entity, UoW, FileDialogService, CommonServices, UserRepository);
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
					guiltyItemsViewModel = new GuiltyItemsViewModel(Entity, UoW, CommonServices, SubdivisionRepository, _employeeSelectorFactory);
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

		public IOrderSelectorFactory OrderSelectorFactory { get; }
        public IEmployeeJournalFactory EmployeeJournalFactory { get; }
        public ICounterpartyJournalFactory CounterpartyJournalFactory { get; }
        public IDeliveryPointJournalFactory DeliveryPointJournalFactory { get; }
        public ISubdivisionJournalFactory SubdivisionJournalFactory { get; }
        public IGtkTabsOpener GtkDialogsOpener { get; }
        public IUndeliveredOrdersJournalOpener UndeliveredOrdersJournalOpener { get; }
		public INomenclatureJournalFactory NomenclatureSelector { get; }
		public IUndeliveredOrdersRepository UndeliveredOrdersRepository { get; }
	}
}
