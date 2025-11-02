using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Vodovoz.Application.Complaints;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Complaints;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Services;
using Vodovoz.Settings.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;
using VodovozBusiness.Domain.Complaints;

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
		private readonly ISubdivisionSettings _subdivisionSettings;
		private readonly IComplaintService _complaintService;
		private readonly IInteractiveService _interactiveService;
		private readonly IComplaintFileStorageService _complaintFileStorageService;
		private ILifetimeScope _lifetimeScope;
		private IList<ComplaintObject> _complaintObjectSource;
		private ComplaintObject _complaintObject;
		private DelegateCommand _changeDeliveryPointCommand;
		private Employee _currentEmployee;
		private List<ComplaintSource> _complaintSources;
		private IList<ComplaintKind> _complaintKindSource;
		private GuiltyItemsViewModel _guiltyItemsViewModel;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

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
			IEmployeeJournalFactory employeeJournalFactory,
			ISubdivisionSettings subdivisionSettings,
			IComplaintService complaintService,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory,
			IComplaintFileStorageService complaintFileStorageService,
			string phone = null) : base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(attachedFileInformationsViewModelFactory is null)
			{
				throw new ArgumentNullException(nameof(attachedFileInformationsViewModelFactory));
			}

			LifetimeScope = lifetimeScope;
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_subdivisionSettings = subdivisionSettings ?? throw new ArgumentNullException(nameof(subdivisionSettings));
			_complaintService = complaintService ?? throw new ArgumentNullException(nameof(complaintService));
			_interactiveService = commonServices?.InteractiveService ?? throw new ArgumentNullException(nameof(commonServices.InteractiveService));

			_complaintFileStorageService = complaintFileStorageService;
			Entity.ComplaintType = ComplaintType.Client;
			Entity.SetStatus(ComplaintStatuses.NotTakenInProcess);
			ConfigureEntityPropertyChanges();
			Entity.Phone = phone;

			_complaintKinds = _complaintKindSource = UoW.GetAll<ComplaintKind>().Where(k => !k.IsArchive).ToList();

			UserHasOnlyAccessToWarehouseAndComplaints =
				CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.UserPermissions.UserHaveAccessOnlyToWarehouseAndComplaints)
				&& !CommonServices.UserService.GetCurrentUser().IsAdmin;

			TabName = "Новая клиентская рекламация";
			
			InitializeEntryViewModels();
			Entity.PropertyChanged += EntityPropertyChanged;

			CanEditComplaintClassification =
				CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.ComplaintPermissions.CanEditComplaintClassification);

			AttachedFileInformationsViewModel = attachedFileInformationsViewModelFactory.CreateAndInitialize<Complaint, ComplaintFileInformation>(
				UoW,
				Entity,
				complaintFileStorageService,
				_cancellationTokenSource.Token,
				Entity.AddFileInformation,
				Entity.RemoveFileInformation);
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
			else if(orderRating.OnlineOrder != null)
			{
				Entity.Counterparty = orderRating.OnlineOrder.Counterparty;
			}
		}
		
		public void ShowMessage(string message)
		{
			ShowInfoMessage(message);
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

		public Employee CurrentEmployee {
			get {
				if(_currentEmployee == null) {
					_currentEmployee = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}
				return _currentEmployee;
			}
		}

		//так как диалог только для создания рекламации
		public bool CanEdit => PermissionResult.CanCreate;

		public bool CanEditComplaintClassification { get; }
		public AttachedFileInformationsViewModel AttachedFileInformationsViewModel { get; private set; }

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
		}

		protected override bool BeforeSave()
		{
			var checkDuplicatesFromDate = DateTime.Now.AddDays(-1);
			var checkDuplicatesToDate = DateTime.Now;
			var canSave = _complaintService.CheckForDuplicateComplaint(UoW, Entity, checkDuplicatesFromDate, checkDuplicatesToDate);

			return canSave;
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

		public bool UserHasOnlyAccessToWarehouseAndComplaints { get; }
		private IEmployeeJournalFactory EmployeeJournalFactory { get; }

		public override bool Save(bool close)
		{
			if(!base.Save(false))
			{
				return false;
			}

			AddAttachedFilesIfNeeded();
			UpdateAttachedFilesIfNeeded();
			DeleteAttachedFilesIfNeeded();

			return base.Save(close);
		}

		private void AddAttachedFilesIfNeeded()
		{
			var errors = new Dictionary<string, string>();
			var repeat = false;

			if(!AttachedFileInformationsViewModel.FilesToAddOnSave.Any())
			{
				return;
			}

			do
			{
				foreach(var fileName in AttachedFileInformationsViewModel.FilesToAddOnSave)
				{
					var result = _complaintFileStorageService.CreateFileAsync(Entity, fileName,
					new MemoryStream(AttachedFileInformationsViewModel.AttachedFiles[fileName]), _cancellationTokenSource.Token)
						.GetAwaiter()
						.GetResult();


					if(result.IsFailure && !result.Errors.All(x => x.Code == Application.Errors.S3.FileAlreadyExists.ToString()))
					{
						errors.Add(fileName, string.Join(", ", result.Errors.Select(e => e.Message)));
					}
				}

				if(errors.Any())
				{
					repeat = _interactiveService.Question(
						"Не удалось загрузить файлы:\n" +
						string.Join("\n- ", errors.Select(fekv => $"{fekv.Key} - {fekv.Value}")) + "\n" +
						"\n" +
						"Повторить попытку?",
						"Ошибка загрузки файлов");

					errors.Clear();
				}
				else
				{
					repeat = false;
				}
			}
			while(repeat);
		}

		private void UpdateAttachedFilesIfNeeded()
		{
			if(!AttachedFileInformationsViewModel.FilesToUpdateOnSave.Any())
			{
				return;
			}

			foreach(var fileName in AttachedFileInformationsViewModel.FilesToUpdateOnSave)
			{
				_complaintFileStorageService.UpdateFileAsync(Entity, fileName, new MemoryStream(AttachedFileInformationsViewModel.AttachedFiles[fileName]), _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();
			}
		}

		private void DeleteAttachedFilesIfNeeded()
		{
			if(!AttachedFileInformationsViewModel.FilesToDeleteOnSave.Any())
			{
				return;
			}

			foreach(var fileName in AttachedFileInformationsViewModel.FilesToDeleteOnSave)
			{
				_complaintFileStorageService.DeleteFileAsync(Entity, fileName, _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();
			}
		}

		public override void Dispose()
		{
			LifetimeScope = null;
			Entity.PropertyChanged -= EntityPropertyChanged;
			base.Dispose();
		}
	}
}
