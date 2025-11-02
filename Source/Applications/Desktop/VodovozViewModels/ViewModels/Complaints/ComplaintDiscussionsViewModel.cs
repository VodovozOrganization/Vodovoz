using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Complaints;
using Vodovoz.EntityRepositories;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintDiscussionsViewModel : EntityWidgetViewModelBase<Complaint>, IDisposable
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly IEmployeeService _employeeService;
		private readonly IUserRepository _userRepository;
		private readonly INavigationManager _navigationManager;
		private readonly IComplaintDiscussionCommentFileStorageService _complaintDiscussionCommentFileStorageService;
		private readonly IAttachedFileInformationsViewModelFactory _attachedFileInformationsViewModelFactory;
		private DialogViewModelBase _parentTab;

		private Dictionary<int, ComplaintDiscussionViewModel> _viewModelsCache = new Dictionary<int, ComplaintDiscussionViewModel>();

		public ComplaintDiscussionsViewModel(
			Complaint entity,
			IUnitOfWork uow,
			DialogViewModelBase parentTab,
			IFileDialogService fileDialogService,
			IEmployeeService employeeService,
			ICommonServices commonServices,
			IUserRepository userRepository,
			INavigationManager navigationManager,
			IComplaintDiscussionCommentFileStorageService complaintDiscussionCommentFileStorageService,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory)
			: base(entity, commonServices)
		{
			_parentTab = parentTab ?? throw new ArgumentNullException(nameof(parentTab));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_navigationManager = navigationManager;
			_complaintDiscussionCommentFileStorageService = complaintDiscussionCommentFileStorageService ?? throw new ArgumentNullException(nameof(complaintDiscussionCommentFileStorageService));
			_attachedFileInformationsViewModelFactory = attachedFileInformationsViewModelFactory ?? throw new ArgumentNullException(nameof(attachedFileInformationsViewModelFactory));
			UoW = uow;
			ConfigureEntityPropertyChanges();
			FillDiscussionsViewModels();

			AttachSubdivisionCommand = new DelegateCommand(AttachSubdivision, () => CanAttachSubdivision);
			AttachSubdivisionCommand.CanExecuteChangedWith(this, x => x.CanAttachSubdivision);

			AttachSubdivisionByComplaintKindCommand = new DelegateCommand(AttachSubdivisionByComplaintKind, () => CanAttachSubdivision);
			AttachSubdivisionByComplaintKindCommand.CanExecuteChangedWith(this, x => x.CanAttachSubdivision);
		}

		private GenericObservableList<ComplaintDiscussionViewModel> _observableComplaintDiscussionViewModels = new GenericObservableList<ComplaintDiscussionViewModel>();

		public virtual GenericObservableList<ComplaintDiscussionViewModel> ObservableComplaintDiscussionViewModels
		{
			get => _observableComplaintDiscussionViewModels;
			set => SetField(ref _observableComplaintDiscussionViewModels, value);
		}

		public DelegateCommand AttachSubdivisionCommand { get; }
		public DelegateCommand AttachSubdivisionByComplaintKindCommand { get; }

		public bool CanEdit => PermissionResult.CanUpdate;

		public bool CanAttachSubdivision => CanEdit;

		private void ConfigureEntityPropertyChanges()
		{
			Entity.ObservableComplaintDiscussions.ElementAdded += ObservableComplaintDiscussions_ElementAdded;
			Entity.ObservableComplaintDiscussions.ElementRemoved += ObservableComplaintDiscussions_ElementRemoved;
		}

		private void ObservableComplaintDiscussions_ElementAdded(object aList, int[] aIdx)
		{
			FillDiscussionsViewModels();
		}

		private void ObservableComplaintDiscussions_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			FillDiscussionsViewModels();
		}

		private void FillDiscussionsViewModels()
		{
			foreach(ComplaintDiscussion discussion in Entity.ObservableComplaintDiscussions)
			{
				var discussionViewModel = GetDiscussionViewModel(discussion);
				if(!ObservableComplaintDiscussionViewModels.Contains(discussionViewModel))
				{
					ObservableComplaintDiscussionViewModels.Add(discussionViewModel);
				}
			}
		}

		private ComplaintDiscussionViewModel GetDiscussionViewModel(ComplaintDiscussion complaintDiscussion)
		{
			int subdivisionId = complaintDiscussion.Subdivision.Id;

			if(_viewModelsCache.ContainsKey(subdivisionId))
			{
				return _viewModelsCache[subdivisionId];
			}

			var viewModel =
				new ComplaintDiscussionViewModel(
					complaintDiscussion,
					_fileDialogService,
					_employeeService,
					CommonServices,
					UoW,
					_userRepository,
					_complaintDiscussionCommentFileStorageService,
					_attachedFileInformationsViewModelFactory);

			_viewModelsCache.Add(subdivisionId, viewModel);

			return viewModel;
		}

		private void AttachSubdivision()
		{
			var page = _navigationManager.OpenViewModel<SubdivisionsJournalViewModel, Action<SubdivisionFilterViewModel>>(
				_parentTab,
				filter => filter.ExcludedSubdivisionsIds = Entity.ObservableComplaintDiscussions
					.Select(x => x.Subdivision.Id)
					.ToArray(),
				OpenPageOptions.AsSlave,
				vm =>
				{
					vm.SelectionMode = JournalSelectionMode.Single;
					vm.OnSelectResult += OnSubdivisionSelected;
				});
		}

		private void AttachSubdivisionByComplaintKind()
		{
			if(Entity.ComplaintKind == null)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, $"Не выбран вид рекламаций");
				return;
			}

			if(!Entity.ComplaintKind.Subdivisions.Any())
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
					$"У вида рекламации {Entity.ComplaintKind.Name} отсутствуют подключаемые отделы.");
				return;
			}

			string subdivisionString = string.Join(", ", Entity.ComplaintKind.Subdivisions.Select(s => s.Name));

			if(CommonServices.InteractiveService.Question(
				$"Будут подключены следующие отделы: {subdivisionString}.",
				"Подключить?"))
			{
				foreach(var subdivision in Entity.ComplaintKind.Subdivisions)
				{
					Entity.AttachSubdivisionToDiscussions(subdivision);
				}
			}
		}

		private void OnSubdivisionSelected(object sender, JournalSelectedEventArgs e)
		{
			var selected = e.SelectedObjects.OfType<SubdivisionJournalNode>().FirstOrDefault();

			if(selected is null)
			{
				return;
			}

			var subdivision = UoW.GetById<Subdivision>(selected.Id);
			Entity.AttachSubdivisionToDiscussions(subdivision);
		}

		public void Dispose()
		{
			_parentTab = null;
		}
	}
}
