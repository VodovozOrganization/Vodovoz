using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Complaints
{
	public class CreateComplaintViewModel : EntityTabViewModelBase<Complaint>
	{
		private readonly IEntityAutocompleteSelectorFactory employeeSelectorFactory;
        private readonly IFilePickerService filePickerService;
        private List<ComplaintObject> _complaintObjectSource;
        private ComplaintObject _complaintObject;
        private readonly List<ComplaintKind> _complaintKinds;

		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory { get; }
		public IEmployeeService EmployeeService { get; }
		public INomenclatureRepository NomenclatureRepository { get; }
		public IUserRepository UserRepository { get; }

		public CreateComplaintViewModel(
			IEntityUoWBuilder uowBuilder, 
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			IEntityAutocompleteSelectorFactory employeeSelectorFactory,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository,
            IFilePickerService filePickerService,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IGtkTabsOpener gtkDialogsOpener,
			IUndeliveredOrdersJournalOpener undeliveredOrdersJournalOpener,
			string phone = null
			) : base(uowBuilder, unitOfWorkFactory, commonServices)
		{
            this.filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
            this.employeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));
			EmployeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			NomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			UserRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			CounterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			NomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));

			OrderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			CounterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			DeliveryPointJournalFactory = deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			SubdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			GtkDialogsOpener = gtkDialogsOpener ?? throw new ArgumentNullException(nameof(gtkDialogsOpener));
			UndeliveredOrdersJournalOpener = undeliveredOrdersJournalOpener ?? throw new ArgumentNullException(nameof(undeliveredOrdersJournalOpener));

			Entity.ComplaintType = ComplaintType.Client;
			Entity.SetStatus(ComplaintStatuses.Checking);
			ConfigureEntityPropertyChanges();
			Entity.Phone = phone;

			_complaintKinds = UoW.GetAll<ComplaintKind>().ToList();

			TabName = "Новая клиентская рекламация";
		}

		public CreateComplaintViewModel(Counterparty client,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			IEntityAutocompleteSelectorFactory employeeSelectorFactory,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository,
            IFilePickerService filePickerService,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IGtkTabsOpener gtkDialogsOpener,
			IUndeliveredOrdersJournalOpener undeliveredOrdersJournalOpener,
			string phone = null) : this(uowBuilder, unitOfWorkFactory, employeeService, employeeSelectorFactory, counterpartySelectorFactory,
			subdivisionRepository, commonServices, nomenclatureSelectorFactory, nomenclatureRepository, userRepository, filePickerService,
			orderSelectorFactory, employeeJournalFactory, counterpartyJournalFactory, deliveryPointJournalFactory, subdivisionJournalFactory,
			gtkDialogsOpener, undeliveredOrdersJournalOpener, phone)
		{
			Counterparty _client = UoW.GetById<Counterparty>(client.Id);
			Entity.Counterparty = _client;
			Entity.Phone = phone;
		}
		
		public CreateComplaintViewModel(Order order,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			IEntityAutocompleteSelectorFactory employeeSelectorFactory,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository,
            IFilePickerService filePickerService,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IGtkTabsOpener gtkDialogsOpener,
			IUndeliveredOrdersJournalOpener undeliveredOrdersJournalOpener,
			string phone = null) : this
		(uowBuilder,unitOfWorkFactory,employeeService,employeeSelectorFactory,counterpartySelectorFactory,subdivisionRepository,commonServices,
			nomenclatureSelectorFactory,nomenclatureRepository,userRepository,filePickerService, orderSelectorFactory, employeeJournalFactory,
			counterpartyJournalFactory, deliveryPointJournalFactory, subdivisionJournalFactory,  gtkDialogsOpener, undeliveredOrdersJournalOpener, phone)
		{
			Order _order = UoW.GetById<Order>(order.Id);
			Entity.Order = _order;
			Entity.Counterparty = _order.Client;
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
                    filesViewModel = new ComplaintFilesViewModel(Entity, UoW, filePickerService, CommonServices);
                }
                return filesViewModel;
            }
        }

        //так как диалог только для создания рекламации
        public bool CanEdit => PermissionResult.CanCreate;

		public bool CanSelectDeliveryPoint => Entity.Counterparty != null;

		private List<ComplaintSource> complaintSources;
		private readonly ISubdivisionRepository subdivisionRepository;

		public IEnumerable<ComplaintSource> ComplaintSources {
			get {
				if(complaintSources == null) {
					complaintSources = UoW.GetAll<ComplaintSource>().ToList();
				}
				return complaintSources;
			}
		}

		List<ComplaintKind> complaintKindSource;
		public List<ComplaintKind> ComplaintKindSource {
			get {
				if(complaintKindSource == null)
					complaintKindSource = UoW.GetAll<ComplaintKind>().Where(k => !k.IsArchive).ToList();
				if(Entity.ComplaintKind != null && Entity.ComplaintKind.IsArchive)
					complaintKindSource.Add(UoW.GetById<ComplaintKind>(Entity.ComplaintKind.Id));

				return complaintKindSource;
			}
			set
			{
				SetField(ref complaintKindSource, value);
			}
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

		public IEnumerable<ComplaintObject> ComplaintObjectSource
		{
			get
			{
				if(_complaintObjectSource == null)
				{
					_complaintObjectSource = UoW.GetAll<ComplaintObject>().Where(x => !x.IsArchive).ToList();
				}

				return _complaintObjectSource;
			}
		}

		private GuiltyItemsViewModel guiltyItemsViewModel;
		public GuiltyItemsViewModel GuiltyItemsViewModel {
			get {
				if(guiltyItemsViewModel == null) {
					guiltyItemsViewModel = new GuiltyItemsViewModel(Entity, UoW, CommonServices, subdivisionRepository, employeeSelectorFactory);
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

        public IOrderSelectorFactory OrderSelectorFactory { get; }
        public IEmployeeJournalFactory EmployeeJournalFactory { get; }
        public ICounterpartyJournalFactory CounterpartyJournalFactory { get; }
        public IDeliveryPointJournalFactory DeliveryPointJournalFactory { get; }
        public ISubdivisionJournalFactory SubdivisionJournalFactory { get; }
        public IGtkTabsOpener GtkDialogsOpener { get; }
        public IUndeliveredOrdersJournalOpener UndeliveredOrdersJournalOpener { get; }
	}
}
