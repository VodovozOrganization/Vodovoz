using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Complaints;
using Vodovoz.EntityRepositories;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Complaints
{
	public class ComplaintDiscussionsViewModel : EntityWidgetViewModelBase<Complaint>
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly IEmployeeService _employeeService;
		private readonly IUserRepository _userRepository;
		private readonly INavigationManager _navigationManager;

		public ComplaintDiscussionsViewModel(
			Complaint entity,
			IUnitOfWork uow,
			IFileDialogService fileDialogService,
			IEmployeeService employeeService,
			ICommonServices commonServices,
			IUserRepository userRepository,
			INavigationManager navigationManager) : base(entity, commonServices)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_navigationManager = navigationManager;

			UoW = uow;
			CreateCommands();
			ConfigureEntityPropertyChanges();
			FillDiscussionsViewModels();
		}

		public bool CanEdit => PermissionResult.CanUpdate;

		private Dictionary<int, ComplaintDiscussionViewModel> viewModelsCache = new Dictionary<int, ComplaintDiscussionViewModel>();

		private void ConfigureEntityPropertyChanges()
		{
			Entity.ObservableComplaintDiscussions.ElementAdded += ObservableComplaintDiscussions_ElementAdded;
			Entity.ObservableComplaintDiscussions.ElementRemoved += ObservableComplaintDiscussions_ElementRemoved;
		}

		void ObservableComplaintDiscussions_ElementAdded(object aList, int[] aIdx)
		{
			FillDiscussionsViewModels();
		}

		void ObservableComplaintDiscussions_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			FillDiscussionsViewModels();
		}

		private void FillDiscussionsViewModels()
		{
			foreach(ComplaintDiscussion discussion in Entity.ObservableComplaintDiscussions) {
				var discussionViewModel = GetDiscussionViewModel(discussion);
				if(!ObservableComplaintDiscussionViewModels.Contains(discussionViewModel)) {
					ObservableComplaintDiscussionViewModels.Add(discussionViewModel);
				}
			}
		}

		private ComplaintDiscussionViewModel GetDiscussionViewModel(ComplaintDiscussion complaintDiscussion)
		{
			int subdivisionId = complaintDiscussion.Subdivision.Id;

			if(viewModelsCache.ContainsKey(subdivisionId))
			{
				return viewModelsCache[subdivisionId];
			}

			var viewModel =
				new ComplaintDiscussionViewModel(
					complaintDiscussion, _fileDialogService, _employeeService, CommonServices, UoW, _userRepository);

			viewModelsCache.Add(subdivisionId, viewModel);
			return viewModel;
		}

		GenericObservableList<ComplaintDiscussionViewModel> observableComplaintDiscussionViewModels = new GenericObservableList<ComplaintDiscussionViewModel>();

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ComplaintDiscussionViewModel> ObservableComplaintDiscussionViewModels {
			get => observableComplaintDiscussionViewModels;
			set => SetField(ref observableComplaintDiscussionViewModels, value, () => ObservableComplaintDiscussionViewModels);
		}

		#region Commands

		private void CreateCommands()
		{
			CreateAttachSubdivisionCommand();
			CreateAttachSubdivisionByComplaintKindCommand();
		}

		#region AttachSubdivisionCommand

		public bool CanAttachSubdivision => CanEdit;

		public DelegateCommand AttachSubdivisionCommand { get; private set; }

		private void CreateAttachSubdivisionCommand()
		{
			AttachSubdivisionCommand = new DelegateCommand(
				() => {
					var filter = new SubdivisionFilterViewModel();
					filter.ExcludedSubdivisionsIds = Entity.ObservableComplaintDiscussions.Select(x => x.Subdivision.Id).ToArray();

					var page = _navigationManager.OpenViewModel<SubdivisionsJournalViewModel>(null);

					page.ViewModel.SelectionMode = JournalSelectionMode.Single;

					page.ViewModel.OnSelectResult += (s, e) =>
					{
						SubdivisionJournalNode selected = e.SelectedObjects.Where(x => x is SubdivisionJournalNode).Select(x => (x as SubdivisionJournalNode)).FirstOrDefault();

						Subdivision subdivision = UoW.GetById<Subdivision>(selected.Id);
						Entity.AttachSubdivisionToDiscussions(subdivision);
					};
				},
				() => CanAttachSubdivision
			);
			AttachSubdivisionCommand.CanExecuteChangedWith(this, x => x.CanAttachSubdivision);
		}

		#endregion AttachSubdivisionCommand

		#region AttachSubdivisionByComplaintKindCommand

		public DelegateCommand AttachSubdivisionByComplaintKindCommand { get; private set; }

		private void CreateAttachSubdivisionByComplaintKindCommand()
		{
			AttachSubdivisionByComplaintKindCommand = new DelegateCommand(
				() =>
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
						$"Будут подключены следующие отделы: { subdivisionString }.",
						"Подключить?")
					)
					{
						foreach(var subdivision in Entity.ComplaintKind.Subdivisions)
						{
							Entity.AttachSubdivisionToDiscussions(subdivision);
						}
					}
				},
				() => CanAttachSubdivision
			);
			AttachSubdivisionByComplaintKindCommand.CanExecuteChangedWith(this, x => x.CanAttachSubdivision);
		}

		#endregion AttachSubdivisionByComplaintKindCommand

		#endregion Commands
	}
}
