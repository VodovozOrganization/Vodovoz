using QS.Commands;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Organizations;
using Vodovoz.Domain.Organizations;
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
		private DelegateCommand _saveEditingVersionCommand;
		private DelegateCommand _cancelEditingVersionCommand;
		private DelegateCommand _editVersionCommand;
		private DelegateCommand _addNewVersioCommand;
		private DelegateCommand _changeVersionStartDateCommand;

		public OrganizationVersionsViewModel(
			Organization entity,
			ICommonServices commonServices,
			IOrganizationVersionsController organizationVersionsController,
			IEmployeeJournalFactory employeeJournalFactory)
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
		}

		public IEntityAutocompleteSelectorFactory LeaderSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory AccountantSelectorFactory { get; }
		public bool IsEditAvailable => SelectedOrganizationVersion != null;
		public bool IsNewOrganization => Entity.Id == 0;
		public Employee Leader { get => _leader; set => SetField(ref _leader, value); }
		public Employee Accountant { get => _accountant; set => SetField(ref _accountant, value); }
		public string Address { get => _address; set => SetField(ref _address, value); }
		public string JurAddress { get => _jurAddress; set => SetField(ref _jurAddress, value); }
		public bool IsEditVisible { get => _isEditVisible; set => SetField(ref _isEditVisible, value); }

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
				SelectedOrganizationVersion.Accountant = Accountant;
				SelectedOrganizationVersion.Leader = Leader;
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
			Address = string.Empty;
			JurAddress = string.Empty; ;
		}
	}
}
