using Autofac;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Complaints;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Services;
using Vodovoz.Settings.Organizations;
using Vodovoz.TempAdapters;
using VodovozBusiness.Domain.Complaints;

namespace Vodovoz.ViewModels.Complaints
{
	public class CreateInnerComplaintViewModel : EntityTabViewModelBase<Complaint>
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IEmployeeService _employeeService;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IFileDialogService _fileDialogService;
		private readonly IUserRepository _userRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IComplaintFileStorageService _complaintFileStorageService;
		private readonly IInteractiveService _interactiveService;
		private readonly ISubdivisionSettings _subdivisionSettings;
		private IList<ComplaintObject> _complaintObjectSource;
		private ComplaintObject _complaintObject;
		private readonly IList<ComplaintKind> _complaintKinds;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public CreateInnerComplaintViewModel(
			IEntityUoWBuilder uoWBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IEmployeeService employeeService,
			ISubdivisionRepository subdivisionRepository,
			ICommonServices commonServices,
			IEmployeeJournalFactory employeeJournalFactory,
			IFileDialogService fileDialogService,
			IUserRepository userRepository,
			ISubdivisionSettings subdivisionSettings,
			IRouteListItemRepository routeListItemRepository,
			IComplaintFileStorageService complaintFileStorageService,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory)
			: base(uoWBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(attachedFileInformationsViewModelFactory is null)
			{
				throw new ArgumentNullException(nameof(attachedFileInformationsViewModelFactory));
			}

			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_subdivisionSettings = subdivisionSettings ?? throw new ArgumentNullException(nameof(subdivisionSettings));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_complaintFileStorageService = complaintFileStorageService ?? throw new ArgumentNullException(nameof(complaintFileStorageService));
			_interactiveService = commonServices?.InteractiveService ?? throw new ArgumentNullException(nameof(commonServices.InteractiveService));

			Entity.ComplaintType = ComplaintType.Inner;
			Entity.SetStatus(ComplaintStatuses.NotTakenInProcess);

			_complaintKinds = complaintKindSource = UoW.GetAll<ComplaintKind>().Where(k => !k.IsArchive).ToList();

			TabName = "Новая внутреняя рекламация";

			AttachedFileInformationsViewModel = attachedFileInformationsViewModelFactory.CreateAndInitialize<Complaint, ComplaintFileInformation>(
				UoW,
				Entity,
				_complaintFileStorageService,
				_cancellationTokenSource.Token,
				Entity.AddFileInformation,
				Entity.RemoveFileInformation);

			Entity.PropertyChanged += EntityPropertyChanged;
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

		//так как диалог только для создания рекламации
		public bool CanEdit => PermissionResult.CanCreate;

		private Employee currentEmployee;
		public Employee CurrentEmployee {
			get {
				if(currentEmployee == null) {
					currentEmployee = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}
				return currentEmployee;
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
		public GuiltyItemsViewModel GuiltyItemsViewModel
		{
			get
			{
				if(guiltyItemsViewModel == null)
				{
					guiltyItemsViewModel =
						new GuiltyItemsViewModel(Entity, UoW, this, _lifetimeScope, CommonServices, _subdivisionRepository, _employeeJournalFactory, _subdivisionSettings);
				}

				return guiltyItemsViewModel;
			}
		}

		public AttachedFileInformationsViewModel AttachedFileInformationsViewModel { get; }

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
	}
}
