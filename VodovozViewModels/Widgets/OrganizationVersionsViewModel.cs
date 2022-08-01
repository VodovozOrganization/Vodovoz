using QS.Commands;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Organizations;
using Vodovoz.Domain.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.ViewModels.Widgets.Organizations
{
	public class OrganizationVersionsViewModel : EntityWidgetViewModelBase<Organization>
	{
		private DateTime? _selectedDate;
		private OrganizationVersion _selectedOrganizationVersion;
		private bool _isVisible;
		private bool _isEditVisible;
		private readonly IOrganizationVersionsController _organizationVersionsController;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly INavigationManager _navigationManager;
		private readonly ITdiTab _parentTab;
		private SaveDto _saveDtoNode;
		private OrganizationVersion _currentVersion;
		private Employee _leader;
		private Employee _accountant;
		private string _address;
		private string _jurAddress;
		private DelegateCommand _saveEditCommand;
		private DelegateCommand _cancelEditCommand;
		private DelegateCommand _editCommand;

		public Employee Leader { get => _leader; set => SetField(ref _leader, value); }
		public Employee Accountant { get => _accountant; set => SetField(ref _accountant, value); }
		public string Address { get => _address; set => SetField(ref _address, value); }
		public string JurAddress { get => _jurAddress; set => SetField(ref _jurAddress, value); }

		public OrganizationVersionsViewModel(Organization entity, ICommonServices commonServices, IOrganizationVersionsController organizationVersionsController,
			IEmployeeJournalFactory employeeJournalFactory/*, ITdiTab parentTab*/
			, INavigationManager navigationManager)
			: base(entity, commonServices)
		{
			//SelectedOrganizationVersion = new OrganizationVersion();
			_organizationVersionsController = organizationVersionsController ?? throw new ArgumentNullException(nameof(organizationVersionsController));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_navigationManager = navigationManager;
			//_parentTab = parentTab ?? throw new ArgumentNullException(nameof(parentTab)); ;
			CanRead = PermissionResult.CanRead;
			CanCreate = PermissionResult.CanCreate && Entity.Id == 0
				|| commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_organization_version");
			CanEdit = commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_organization_version_date");

			//CurrentVersion = new OrganizationVersion();

			if(IsNewOrganization)
			{
				SelectedDate = DateTime.Now.Date;
			}

			//OrganizationVersionViewModel = new OrganizationVersionViewModel(new OrganizationVersion(), CommonServices, _employeeJournalFactory);
			//OrganizationVersionViewModel = new OrganizationVersionViewModel();

			LeaderSelectorFactory = (employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingEmployeeAutocompleteSelectorFactory();

			AccountantSelectorFactory = (employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingEmployeeAutocompleteSelectorFactory();
		}

		public IEntityAutocompleteSelectorFactory LeaderSelectorFactory { get; private set; }

		public void Save(/*OrganizationVersion organizationVersion*/)
		{
			//OrganizationVersionViewModel = new OrganizationVersionViewModel(/*EntityUoWBuilder.ForOpen(organizationVersion.Id),*/ organizationVersion, CommonServices, _employeeJournalFactory/*, organizationVersion*/);
			//var vm = new OrganizationVersionViewModel(CommonServices, organizationVersion, _employeeJournalFactory/*, organizationVersion*/);
			//_parentTab.TabParent.AddSlaveTab(_parentTab, vm);
			//_navigationManager.OpenViewModel<OrganizationVersionViewModel, ICommonServices, OrganizationVersion, IEmployeeJournalFactory>(vm,CommonServices, organizationVersion, _employeeJournalFactory);

			IsEditVisible = false;


			SelectedOrganizationVersion.Accountant = Accountant;
			SelectedOrganizationVersion.Leader = Leader;
			SelectedOrganizationVersion.Address = Address;
			SelectedOrganizationVersion.JurAddress = JurAddress;

		}
		public IEntityAutocompleteSelectorFactory AccountantSelectorFactory { get; set; }
		public SaveDto SaveDtoNode { get => _saveDtoNode; set => SetField(ref _saveDtoNode, value); }

		public class SaveDto : PropertyChangedBase
		{
			private Employee _leader;
			private Employee _accountant;
			private string _address;
			private string _jurAddress;

			public SaveDto(OrganizationVersion organizationVersion)
			{
				//_organizationVersion = organizationVersion;
				Leader = organizationVersion.Leader;
				Accountant = organizationVersion.Accountant;
				Address = organizationVersion.Address;
				JurAddress = organizationVersion.JurAddress;
			}

			public Employee Leader { get => _leader; set => SetField(ref _leader, value); }
			public Employee Accountant { get => _accountant; set => SetField(ref _accountant, value); }
			public string Address { get => _address; set => SetField(ref _address, value); }
			public string JurAddress { get => _jurAddress; set => SetField(ref _jurAddress, value); }
		}


		//public OrganizationVersionViewModel OrganizationVersionViewModel
		//	{
		//		get => _organizationVersionViewModel;
		//		set => SetField(ref _organizationVersionViewModel, value);
		//	}

		public void CancelEdit()
		{
			IsEditVisible = false;
		}

		public virtual DateTime? SelectedDate
		{
			get => _selectedDate;
			set
			{
				if(SetField(ref _selectedDate, value))
				{
					OnPropertyChanged(nameof(CanAddNewVersion));
					OnPropertyChanged(nameof(CanChangeVersionDate));
				}
			}
		}

		[PropertyChangedAlso(nameof(IsEditAvailable))]
		public virtual OrganizationVersion SelectedOrganizationVersion
		{
			get => _selectedOrganizationVersion;
			set
			{
				if(SetField(ref _selectedOrganizationVersion, value))
				{
					OnPropertyChanged(nameof(CanChangeVersionDate));
				}
			}
		}

		public virtual bool CanRead { get; }
		public virtual bool CanCreate { get; }
		public virtual bool CanEdit { get; }

		public bool CanAddNewVersion =>
			CanCreate
			&& SelectedDate.HasValue
			&& Entity.OrganizationVersions.All(x => x.Id != 0)
			&& _organizationVersionsController.IsValidDateForNewOrganizationVersion(SelectedDate.Value);

		public bool CanChangeVersionDate =>
			SelectedDate.HasValue
			&& SelectedOrganizationVersion != null
			&& (CanEdit || SelectedOrganizationVersion.Id == 0)
			&& _organizationVersionsController.IsValidDateForVersionStartDateChange(SelectedOrganizationVersion, SelectedDate.Value);

		public void EditVersion(/*OrganizationVersion organizationVersion*/)
		{
			//OrganizationVersionViewModel = new OrganizationVersionViewModel(/*EntityUoWBuilder.ForOpen(organizationVersion.Id),*/ organizationVersion, CommonServices, _employeeJournalFactory/*, organizationVersion*/);
			//var vm = new OrganizationVersionViewModel(CommonServices, organizationVersion, _employeeJournalFactory/*, organizationVersion*/);
			//_parentTab.TabParent.AddSlaveTab(_parentTab, vm);
			//_navigationManager.OpenViewModel<OrganizationVersionViewModel, ICommonServices, OrganizationVersion, IEmployeeJournalFactory>(vm,CommonServices, organizationVersion, _employeeJournalFactory);
			IsEditVisible = true;
			Accountant = SelectedOrganizationVersion.Accountant;
			Leader = SelectedOrganizationVersion.Leader;
			Address = SelectedOrganizationVersion.Address;
			JurAddress = SelectedOrganizationVersion.JurAddress;



			//CurrentVersion = organizationVersion;
			//OnPropertyChanged(nameof(CurrentVersion));
			//OnPropertyChanged(nameof(CurrentVersion.Accountant));
			//OnPropertyChanged(nameof(CurrentVersion.Leader));
			//OnPropertyChanged(nameof(CurrentVersion.Address));
			//OnPropertyChanged(nameof(CurrentVersion.JurAddress));
			//SaveDtoNode = new SaveDto(organizationVersion);
			//OnPropertyChanged(nameof(SaveDtoNode));
			//OnPropertyChanged(nameof(SaveDtoNode.Accountant));
			//OnPropertyChanged(nameof(SaveDtoNode.Leader));
			//OnPropertyChanged(nameof(SaveDtoNode.Address));
			//OnPropertyChanged(nameof(SaveDtoNode.JurAddress));
			//OnPropertyChanged(nameof(Entity));
		}

		public void AddNewOrganizationVersion()
		{
			if(SelectedDate == null)
			{
				return;
			}

			SelectedOrganizationVersion =  _organizationVersionsController.CreateAndAddVersion(SelectedDate);
			EditCommand.Execute();


			OnPropertyChanged(nameof(CanAddNewVersion));
			OnPropertyChanged(nameof(CanChangeVersionDate));
		}

		public void ChangeVersionStartDate()
		{
			if(SelectedDate == null || SelectedOrganizationVersion == null)
			{
				return;
			}
			_organizationVersionsController.ChangeVersionStartDate(SelectedOrganizationVersion, SelectedDate.Value);

			OnPropertyChanged(nameof(CanAddNewVersion));
			OnPropertyChanged(nameof(CanChangeVersionDate));
		}

		public bool IsNewOrganization => Entity.Id == 0;

		public bool IsEditVisible { get => _isEditVisible; set => SetField(ref _isEditVisible, value); }
		
		public bool IsEditAvailable => SelectedOrganizationVersion != null;
		//public OrganizationVersion CurrentVersion { get => _currentVersion; set => SetField(ref _currentVersion, value); }

		//public bool IsVisible 
		//{
		//	get => _isVisible;
		//	set => SetField(ref _isVisible, value);
		//}

		#region Commands

		public DelegateCommand SaveEditCommand =>
			_saveEditCommand ?? (_saveEditCommand = new DelegateCommand(() =>
			{
				SelectedOrganizationVersion.Accountant = Accountant;
				SelectedOrganizationVersion.Leader = Leader;
				SelectedOrganizationVersion.Address = Address;
				SelectedOrganizationVersion.JurAddress = JurAddress;

				ClearProperties();
				IsEditVisible = false;
			},
				() => IsEditAvailable
			));

		public DelegateCommand CancelEditCommand =>
			_cancelEditCommand ?? (_cancelEditCommand = new DelegateCommand(() =>
			{
				ClearProperties();
				IsEditVisible = false;
			},
				() => true
			));

		private void ClearProperties()
		{
			Leader = null;
			Accountant = null;
			Address = string.Empty;
			JurAddress = string.Empty; ;
		}

		public DelegateCommand EditCommand =>
			_editCommand ?? (_editCommand = new DelegateCommand(() =>
			{
				IsEditVisible = true;
				Accountant = SelectedOrganizationVersion.Accountant;
				Leader = SelectedOrganizationVersion.Leader;
				Address = SelectedOrganizationVersion.Address;
				JurAddress = SelectedOrganizationVersion.JurAddress;
			},
				() => IsEditAvailable
			));

		#endregion
	}
}
