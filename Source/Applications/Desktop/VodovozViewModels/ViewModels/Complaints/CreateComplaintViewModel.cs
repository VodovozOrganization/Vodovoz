using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.DomainModel.Entity;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Services;
using Vodovoz.Settings.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.ViewModels.Complaints
{
	public class CreateComplaintViewModel : EntityTabViewModelBase<Complaint>
	{
		private readonly IList<ComplaintKind> _complaintKinds;
		private readonly IEmployeeService _employeeService;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IUserRepository _userRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IFileDialogService _fileDialogService;
		private readonly IOrderSelectorFactory _orderSelectorFactory;
		private readonly ISubdivisionSettings _subdivisionSettings;
		private IList<ComplaintObject> _complaintObjectSource;
		private ComplaintObject _complaintObject;
		private DelegateCommand _changeDeliveryPointCommand;
		private Employee _currentEmployee;
		private List<ComplaintSource> _complaintSources;
		private IList<ComplaintKind> _complaintKindSource;
		private ComplaintFilesViewModel _filesViewModel;
		private GuiltyItemsViewModel _guiltyItemsViewModel;

		public CreateComplaintViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IEmployeeService employeeService,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			IUserRepository userRepository,
			IRouteListItemRepository routeListItemRepository,
			IFileDialogService fileDialogService,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISubdivisionSettings subdivisionSettings,
			string phone = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			LifetimeScope = lifetimeScope;
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			DeliveryPointJournalFactory = deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			_subdivisionSettings = subdivisionSettings ?? throw new ArgumentNullException(nameof(subdivisionSettings));

			Entity.ComplaintType = ComplaintType.Client;
			Entity.SetStatus(ComplaintStatuses.Checking);
			ConfigureEntityPropertyChanges();
			Entity.Phone = phone;

			_complaintKinds = _complaintKindSource = UoW.GetAll<ComplaintKind>().Where(k => !k.IsArchive).ToList();

			UserHasOnlyAccessToWarehouseAndComplaints =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !ServicesConfig.CommonServices.UserService.GetCurrentUser().IsAdmin;

			TabName = "Новая клиентская рекламация";
			
			InitializeOrderAutocompleteSelectorFactory(orderSelectorFactory);
			InitializeEntryViewModels();
			Entity.PropertyChanged += EntityPropertyChanged;
		}

		public void SetOrder(int orderId)
		{
			var currentOrder = UoW.GetById<Order>(orderId);
			SetOrder(currentOrder);
		}

		public void SetCounterparty(int clientId)
		{
			var currentClient = UoW.GetById<Counterparty>(clientId);
			Entity.Counterparty = currentClient;
		}
		
		public void SetOrderRating(int orderRatingId)
		{
			var orderRating = UoW.GetById<OrderRating>(orderRatingId);
			Entity.OrderRating = orderRating;

			if(orderRating.Order != null)
			{
				SetOrder(orderRating.Order);
			}
			else
			{
				Entity.Counterparty = orderRating.OnlineOrder.Counterparty;
			}
		}

		private void SetOrder(Order order)
		{
			Entity.Order = order;
			Entity.Counterparty = order.Client;
			ChangeDeliveryPointCommand.Execute();
		}

		private void EntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.Order))
			{
				if(Entity.Order is null)
				{
					Entity.Driver = null;
					return;
				}

				var routeList = _routeListItemRepository.GetRouteListItemForOrder(UoW, Entity.Order)?.RouteList;

				if(routeList is null)
				{
					Entity.Driver = null;
					return;
				}

				Entity.Driver = routeList.Driver;
			}
		}

		private void InitializeOrderAutocompleteSelectorFactory(IOrderSelectorFactory orderSelectorFactory)
		{
			var orderFilter = LifetimeScope.Resolve<OrderJournalFilterViewModel>();
			
			if(Entity.Counterparty != null)
			{
				orderFilter.RestrictCounterparty = Entity.Counterparty;
			}
			
			OrderAutocompleteSelectorFactory =
				(orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory)))
				.CreateOrderAutocompleteSelectorFactory(orderFilter);
		}

		public Employee CurrentEmployee {
			get {
				if(_currentEmployee == null) {
					_currentEmployee = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}
				return _currentEmployee;
			}
		}

		public ComplaintFilesViewModel FilesViewModel
		{
			get
			{
				if (_filesViewModel == null)
				{
					_filesViewModel = new ComplaintFilesViewModel(Entity, UoW, _fileDialogService, CommonServices, _userRepository);
				}
				return _filesViewModel;
			}
		}

		//так как диалог только для создания рекламации
		[PropertyChangedAlso(nameof(CanChangeOrder))]
		public bool CanEdit => PermissionResult.CanCreate;
		public bool CanChangeOrder => CanEdit && Entity.OrderRating is null;

		public bool CanSelectDeliveryPoint => Entity.Counterparty != null;
		
		public IEnumerable<ComplaintSource> ComplaintSources {
			get {
				if(_complaintSources == null) {
					_complaintSources = UoW.GetAll<ComplaintSource>().ToList();
				}
				return _complaintSources;
			}
		}

		public IList<ComplaintKind> ComplaintKindSource {
			get => _complaintKindSource;
			set => SetField(ref _complaintKindSource, value);
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

		public ILifetimeScope LifetimeScope { get; private set; }
		public IEntityEntryViewModel OrderRatingEntryViewModel { get; private set; }
		//public IEntityEntryViewModel CounterpartyEntryViewModel { get; private set; }

		public GuiltyItemsViewModel GuiltyItemsViewModel {
			get {
				if(_guiltyItemsViewModel == null) {
					_guiltyItemsViewModel = new GuiltyItemsViewModel(
						Entity,
						UoW,
						this,
						LifetimeScope,
						CommonServices,
						_subdivisionRepository,
						EmployeeJournalFactory,
						_subdivisionSettings);
				}

				return _guiltyItemsViewModel;
			}
		}

		protected override bool BeforeValidation()
		{
			if(UoW.IsNew) {
				Entity.CreatedBy = CurrentEmployee;
				Entity.CreationDate = DateTime.Now;
				Entity.PlannedCompletionDate = DateTime.Today;
			}
			Entity.ChangedBy = CurrentEmployee;
			Entity.ChangedDate = DateTime.Now;

			return base.BeforeValidation();
		}

		private void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(
				e => e.Counterparty,
				() => CanSelectDeliveryPoint
			);
			
			SetPropertyChangeRelation(
				e => e.OrderRating,
				() => CanChangeOrder
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
		
		private void InitializeEntryViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<Complaint>(this, Entity, UoW, NavigationManager, LifetimeScope);

			OrderRatingEntryViewModel =
				builder
					.ForProperty(x => x.OrderRating)
					.UseViewModelDialog<OrderRatingViewModel>()
					.UseViewModelJournalAndAutocompleter<OrdersRatingsJournalViewModel>()
					.Finish();
			OrderRatingEntryViewModel.IsEditable = false;
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

		public IEntityAutocompleteSelectorFactory CounterpartyAutocompleteSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory OrderAutocompleteSelectorFactory { get; private set; }
		public bool UserHasOnlyAccessToWarehouseAndComplaints { get; }
		private IEmployeeJournalFactory EmployeeJournalFactory { get; }
		private IDeliveryPointJournalFactory DeliveryPointJournalFactory { get; }

		public override void Dispose()
		{
			LifetimeScope = null;
			Entity.PropertyChanged -= EntityPropertyChanged;
			base.Dispose();
		}
	}
}
