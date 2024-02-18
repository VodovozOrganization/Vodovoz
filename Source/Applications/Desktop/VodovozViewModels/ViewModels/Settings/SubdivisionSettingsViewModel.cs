using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Settings.Common;

namespace Vodovoz.ViewModels.ViewModels.Settings
{
	public class SubdivisionSettingsViewModel : WidgetViewModelBase
	{
		private readonly ICommonServices _commonServices;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly INavigationManager _navigationManager;
		private readonly IGeneralSettings _generalSettingsSettings;
		private GenericObservableList<Subdivision> _observableSubdivisions = new GenericObservableList<Subdivision>();
		private Subdivision _selectedSubdivision;
		private readonly List<int> _subdivisionsToAdd = new List<int>();
		private readonly List<int> _subdivisionsToRemoves = new List<int>();

		public SubdivisionSettingsViewModel(ICommonServices commonServices, IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager, IGeneralSettings generalSettingsSettings, string parameterName)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_navigationManager = navigationManager;
			_generalSettingsSettings = generalSettingsSettings ?? throw new ArgumentNullException(nameof(generalSettingsSettings));

			AddSubdivisionCommand = new DelegateCommand(AddSubdivision);
			RemoveSubdivisionCommand = new DelegateCommand(RemoveSubdivision, () => CanRemove);
			SaveSubdivisionsCommand = new DelegateCommand (SaveSubdivisions);
			ShowSubdivisionsInfoCommand = new DelegateCommand(ShowSubdivisionsToInformComplaintHasNoDriverInfo);
			ParameterName = parameterName;
		}

		private void AddSubdivision()
		{
			var page = _navigationManager.OpenViewModel<SubdivisionsJournalViewModel>(null);
			page.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
			page.ViewModel.OnSelectResult += OnSubdivisionsToAddSelected;
		}

		private void OnSubdivisionsToAddSelected(object sender, JournalSelectedEventArgs e)
		{
			if(!(sender is SubdivisionsJournalViewModel viewModel))
			{
				return;
			}

			var selectedIds = e.SelectedObjects.Select(x => ((SubdivisionJournalNode)x).Id);

			if(!selectedIds.Any())
			{
				return;
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot())
			{
				unitOfWork.Session.DefaultReadOnly = true;

				foreach(var id in selectedIds)
				{
					if(!_subdivisionsToAdd.Contains(id)
						&& !_subdivisionsToRemoves.Contains(id))
					{
						_subdivisionsToAdd.Add(id);
					}

					if(_subdivisionsToRemoves.Contains(id))
					{
						_subdivisionsToRemoves.Remove(id);
					}
				}

				var subdivisionIdToRetrieve = _subdivisionsToAdd.Except(ObservableSubdivisions.Select(x => x.Id));

				var retrievedSubdivisions = unitOfWork.Session.Query<Subdivision>()
					.Where(subdivision => subdivisionIdToRetrieve.Contains(subdivision.Id))
					.ToList();

				foreach(var subdivision in retrievedSubdivisions)
				{
					ObservableSubdivisions.Add(subdivision);
				}
			}

			viewModel.OnSelectResult -= OnSubdivisionsToAddSelected;
		}

		private void RemoveSubdivision()
		{
			var currentlySelected = SelectedSubdivision.Id;
			var itemToRemove = ObservableSubdivisions.FirstOrDefault(x => x.Id == currentlySelected);

			if(itemToRemove is null)
			{
				return;
			}

			if(!_subdivisionsToRemoves.Contains(currentlySelected)
				&& !_subdivisionsToAdd.Contains(currentlySelected))
			{
				_subdivisionsToRemoves.Add(currentlySelected);
			}

			if(_subdivisionsToAdd.Contains(currentlySelected))
			{
				_subdivisionsToAdd.Remove(currentlySelected);
			}

			ObservableSubdivisions.Remove(itemToRemove);
		}

		private void SaveSubdivisions()
		{
			_generalSettingsSettings.UpdateSubdivisionsForParameter(_subdivisionsToAdd , _subdivisionsToRemoves, ParameterName);

			_subdivisionsToAdd.Clear();
			_subdivisionsToRemoves.Clear();
		}

		private void ShowSubdivisionsToInformComplaintHasNoDriverInfo() => _commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, Info);
		
		public string ParameterName { get;}

		public GenericObservableList<Subdivision> ObservableSubdivisions
		{
			get => _observableSubdivisions;
			set => SetField(ref _observableSubdivisions, value);
		}

		[PropertyChangedAlso(nameof(CanRemove))]
		public Subdivision SelectedSubdivision
		{
			get => _selectedSubdivision;
			set => SetField(ref _selectedSubdivision, value);
		}

		public bool CanRemove => CanEdit && SelectedSubdivision != null;
		public bool CanEdit { get; set; }
		public string DetailTitle { get; set; }
		public string MainTitle { get; set; }
		public string Info { get; set; }
		public DelegateCommand AddSubdivisionCommand { get; }
		public DelegateCommand RemoveSubdivisionCommand { get; }
		public DelegateCommand SaveSubdivisionsCommand { get; }
		public DelegateCommand ShowSubdivisionsInfoCommand { get; }
	}
}
