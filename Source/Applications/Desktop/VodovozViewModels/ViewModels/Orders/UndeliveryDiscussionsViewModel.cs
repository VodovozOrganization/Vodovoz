using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Orders
{
	public class UndeliveryDiscussionsViewModel : EntityWidgetViewModelBase<UndeliveredOrder>, IDisposable
	{
		private readonly IEmployeeService _employeeService;
		private readonly IUserRepository _userRepository;
		private readonly INavigationManager _navigationManager;
		private readonly IUndeliveryDiscussionCommentFileStorageService _undeliveryDiscussionCommentFileStorageService;
		private readonly IAttachedFileInformationsViewModelFactory _attachedFileInformationsViewModelFactory;
		private ITdiTab _parentTab;
		private readonly Dictionary<int, UndeliveryDiscussionViewModel> _viewModelsCache = new Dictionary<int, UndeliveryDiscussionViewModel>();
		private GenericObservableList<UndeliveryDiscussionViewModel> _observableUndeliveryDiscussionViewModels = new GenericObservableList<UndeliveryDiscussionViewModel>();

		public UndeliveryDiscussionsViewModel(
			UndeliveredOrder entity,
			IUnitOfWork uow,
			ITdiTab parentTab,
			IEmployeeService employeeService,
			IUserRepository userRepository,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IUndeliveryDiscussionCommentFileStorageService undeliveryDiscussionCommentFileStorageService,
			IAttachedFileInformationsViewModelFactory attachedFileInformationsViewModelFactory)
			: base(entity, commonServices)
		{
			_parentTab = parentTab ?? throw new ArgumentNullException(nameof(parentTab));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_navigationManager = navigationManager;
			_undeliveryDiscussionCommentFileStorageService = undeliveryDiscussionCommentFileStorageService;
			_attachedFileInformationsViewModelFactory = attachedFileInformationsViewModelFactory;
			UoW = uow;
			CreateCommands();
			ConfigureEntityPropertyChanges();
			FillDiscussionsViewModels();
		}
			
		private void ConfigureEntityPropertyChanges()
		{
			Entity.ObservableUndeliveryDiscussions.ElementAdded += OnObservableUndeliveryDiscussionsElementAdded;
			Entity.ObservableUndeliveryDiscussions.ElementRemoved += OnObservableUndeliveryDiscussionsElementRemoved;
		}

		private void OnObservableUndeliveryDiscussionsElementAdded(object aList, int[] aIdx)
		{
			FillDiscussionsViewModels();
		}

		private void OnObservableUndeliveryDiscussionsElementRemoved(object aList, int[] aIdx, object aObject)
		{
			FillDiscussionsViewModels();
		}

		private void FillDiscussionsViewModels()
		{
			foreach(UndeliveryDiscussion discussion in Entity.ObservableUndeliveryDiscussions)
			{
				var discussionViewModel = GetDiscussionViewModel(discussion);

				if(!ObservableUndeliveryDiscussionViewModels.Contains(discussionViewModel))
				{
					ObservableUndeliveryDiscussionViewModels.Add(discussionViewModel);
				}
			}
		}

		private UndeliveryDiscussionViewModel GetDiscussionViewModel(UndeliveryDiscussion complaintDiscussion)
		{
			int subdivisionId = complaintDiscussion.Subdivision.Id;

			if(_viewModelsCache.ContainsKey(subdivisionId))
			{
				return _viewModelsCache[subdivisionId];
			}

			var viewModel =	new UndeliveryDiscussionViewModel(
				complaintDiscussion,
				_employeeService,
				CommonServices,
				UoW,
				_userRepository,
				_undeliveryDiscussionCommentFileStorageService,
				_attachedFileInformationsViewModelFactory);

			_viewModelsCache.Add(subdivisionId, viewModel);

			return viewModel;
		}		

		private void CreateCommands()
		{
			CreateAttachSubdivisionCommand();
		}

		private void CreateAttachSubdivisionCommand()
		{
			AttachSubdivisionCommand = new DelegateCommand(() =>
			{
				var page = (_navigationManager as ITdiCompatibilityNavigation)
					.OpenViewModelOnTdi<SubdivisionsJournalViewModel, Action<SubdivisionFilterViewModel>>(_parentTab, filter =>
						filter.ExcludedSubdivisionsIds = Entity.ObservableUndeliveryDiscussions.Select(x => x.Subdivision.Id).ToArray(),
						OpenPageOptions.AsSlave,
						vm => vm.SelectionMode = JournalSelectionMode.Single);

				page.ViewModel.OnSelectResult += (s, e) =>
				{
					var selected = e.SelectedObjects.OfType<SubdivisionJournalNode>().FirstOrDefault();

					if(selected is null)
					{
						return;
					}

					var subdivision = UoW.GetById<Subdivision>(selected.Id);

					Entity.AttachSubdivisionToDiscussions(subdivision);
				};

				AttachSubdivisionCommand.CanExecuteChangedWith(this, x => x.CanAttachSubdivision);
			});
		}

		public bool CanAttachSubdivision => CanEdit;

		public DelegateCommand AttachSubdivisionCommand { get; private set; }

		public virtual GenericObservableList<UndeliveryDiscussionViewModel> ObservableUndeliveryDiscussionViewModels
		{
			get => _observableUndeliveryDiscussionViewModels;
			set => SetField(ref _observableUndeliveryDiscussionViewModels, value);
		}

		public bool CanEdit => PermissionResult.CanUpdate;

		public void Dispose()
		{
			Entity.ObservableUndeliveryDiscussions.ElementAdded -= OnObservableUndeliveryDiscussionsElementAdded;
			Entity.ObservableUndeliveryDiscussions.ElementRemoved -= OnObservableUndeliveryDiscussionsElementRemoved;

			_parentTab = null;
		}
	}
}
