using QS.Commands;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.StoredResources;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Organizations;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.StoredResourceRepository;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Widgets.Organizations
{
	public class OrganizationVersionsViewModel : EntityWidgetViewModelBase<Organization>
	{
		private DateTime? _selectedDate;
		private OrganizationVersion _selectedOrganizationVersion;
		private bool _isEditVisible;
		private readonly IOrganizationVersionsController _organizationVersionsController;
		private Employee _leader;
		private Employee _accountant;
		private string _address;
		private string _jurAddress;
		private StoredResource _signatureLeader;
		private StoredResource _signatureAccountant;
		private DelegateCommand _saveEditingVersionCommand;
		private DelegateCommand _cancelEditingVersionCommand;
		private DelegateCommand _editVersionCommand;
		private DelegateCommand _addNewVersioCommand;
		private DelegateCommand _changeVersionStartDateCommand;
		private IList<StoredResource> _allSignatures;

		public OrganizationVersionsViewModel(
			Organization entity,
			ICommonServices commonServices,
			IOrganizationVersionsController organizationVersionsController,
			IStoredResourceRepository storedResourceRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			bool isEditable = true)
			: base(entity, commonServices)
		{
			_organizationVersionsController = organizationVersionsController ?? throw new ArgumentNullException(nameof(organizationVersionsController));

			if(IsNewOrganization)
			{
				SelectedDate = DateTime.Now.Date;
			}

			LeaderSelectorFactory = (employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingEmployeeAutocompleteSelectorFactory();

			AccountantSelectorFactory = (employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingEmployeeAutocompleteSelectorFactory();

			var _storedResourceRepository = storedResourceRepository ?? throw new ArgumentNullException(nameof(storedResourceRepository));
			_allSignatures = _storedResourceRepository.GetAllSignatures();

			IsButtonsAvailable = isEditable;
		}

		public IEntityAutocompleteSelectorFactory LeaderSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory AccountantSelectorFactory { get; }
		public bool IsEditAvailable => SelectedOrganizationVersion != null;
		public bool IsNewOrganization => Entity.Id == 0;
		public bool IsButtonsAvailable { get; }

		public Employee Leader
		{
			get => _leader;
			set => SetField(ref _leader, value);
		}

		public Employee Accountant
		{
			get => _accountant;
			set => SetField(ref _accountant, value);
		}
		public IList<StoredResource> AllSignatures
		{
			get => _allSignatures;
			private set => SetField(ref _allSignatures, value);
		}

		public string Address
		{
			get => _address;
			set => SetField(ref _address, value);
		}

		public virtual StoredResource SignatureLeader
		{
			get => _signatureLeader;
			set => SetField(ref _signatureLeader, value);
		}

		public virtual StoredResource SignatureAccountant
		{
			get => _signatureAccountant;
			set => SetField(ref _signatureAccountant, value);
		}

		public string JurAddress
		{
			get => _jurAddress;
			set => SetField(ref _jurAddress, value);
		}

		public bool IsEditVisible
		{
			get => _isEditVisible;
			set => SetField(ref _isEditVisible, value);
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

		public virtual OrganizationVersion SelectedOrganizationVersion
		{
			get => _selectedOrganizationVersion;
			set
			{
				if(SetField(ref _selectedOrganizationVersion, value))
				{
					OnPropertyChanged(nameof(CanChangeVersionDate));
					OnPropertyChanged(nameof(IsEditAvailable));
				}
			}
		}

		public bool CanAddNewVersion =>
			SelectedDate.HasValue
			&& Entity.OrganizationVersions.All(x => x.Id != 0)
			&& _organizationVersionsController.IsValidDateForNewOrganizationVersion(SelectedDate.Value);

		public bool CanChangeVersionDate =>
			SelectedDate.HasValue
			&& _organizationVersionsController.IsValidDateForVersionStartDateChange(SelectedOrganizationVersion, SelectedDate.Value);

		#region Commands

		public DelegateCommand SaveEditingVersionCommand =>
			_saveEditingVersionCommand ?? (_saveEditingVersionCommand = new DelegateCommand(() =>
				{
					var version = new OrganizationVersion
					{
						Accountant = Accountant,
						Leader = Leader,
						SignatureLeader = SignatureLeader,
						SignatureAccountant = SignatureAccountant,
						Address = Address,
						JurAddress = JurAddress
					};

					var validationContext = new ValidationContext(version);
					if(!CommonServices.ValidationService.Validate(version, validationContext))
					{
						return;
					}

					SelectedOrganizationVersion.Accountant = Accountant;
					SelectedOrganizationVersion.Leader = Leader;
					SelectedOrganizationVersion.SignatureLeader = SignatureLeader;
					SelectedOrganizationVersion.SignatureAccountant = SignatureAccountant;
					SelectedOrganizationVersion.Address = Address;
					SelectedOrganizationVersion.JurAddress = JurAddress;

					ClearProperties();
					IsEditVisible = false;
				},
				() => IsEditAvailable
			));

		public DelegateCommand CancelEditingVersionCommand =>
			_cancelEditingVersionCommand ?? (_cancelEditingVersionCommand = new DelegateCommand(() =>
			{
				ClearProperties();
				IsEditVisible = false;
			},
				() => true
			));

		public DelegateCommand EditVersionCommand =>
			_editVersionCommand ?? (_editVersionCommand = new DelegateCommand(() =>
			{
				Accountant = SelectedOrganizationVersion.Accountant;
				SignatureLeader = SelectedOrganizationVersion.SignatureLeader;
				SignatureAccountant = SelectedOrganizationVersion.SignatureAccountant;
				Leader = SelectedOrganizationVersion.Leader;
				Address = SelectedOrganizationVersion.Address;
				JurAddress = SelectedOrganizationVersion.JurAddress;

				IsEditVisible = true;
			},
				() => IsEditAvailable
			));

		public DelegateCommand AddNewVersionCommand =>
			_addNewVersioCommand ?? (_addNewVersioCommand = new DelegateCommand(() =>
			{
				if(SelectedDate == null)
				{
					return;
				}

				SelectedOrganizationVersion = _organizationVersionsController.CreateAndAddVersion(SelectedDate);

				EditVersionCommand.Execute();

				OnPropertyChanged(nameof(CanAddNewVersion));
				OnPropertyChanged(nameof(CanChangeVersionDate));
			},
				() => true
			));

		public DelegateCommand ChangeVersionStartDateCommand =>
			_changeVersionStartDateCommand ?? (_changeVersionStartDateCommand = new DelegateCommand(() =>
			{
				if(SelectedDate == null || SelectedOrganizationVersion == null)
				{
					return;
				}
				_organizationVersionsController.ChangeVersionStartDate(SelectedOrganizationVersion, SelectedDate.Value);

				OnPropertyChanged(nameof(CanAddNewVersion));
				OnPropertyChanged(nameof(CanChangeVersionDate));
			},
				() => IsEditAvailable
			));

		#endregion

		private void ClearProperties()
		{
			Leader = null;
			Accountant = null;
			SignatureLeader = null;
			SignatureAccountant = null;
			Address = string.Empty;
			JurAddress = string.Empty;
		}
	}
}
