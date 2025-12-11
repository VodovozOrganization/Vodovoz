using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Commands;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Organizations;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.ViewModels.Cash;
using VodovozBusiness.Controllers.Cash;

namespace Vodovoz.ViewModels.Widgets.Cash
{
	public class VatRateNomenclatureVersionViewModel : EntityWidgetViewModelBase<Nomenclature>
	{
		private readonly IVatRateVersionController _vatRateVersionController;
		private readonly ViewModelEEVMBuilder<VatRate> _vatRateEEVMBuilder;
		private readonly DialogViewModelBase _parentDialog;
		private readonly bool _isEditable;

		private DateTime? _selectedDate;
		private VatRateVersion _selectedVatRateVersion;
		private bool _isEditVisible;
		private DelegateCommand _saveEditingVersionCommand;
		private DelegateCommand _cancelEditingVersionCommand;
		private DelegateCommand _editVersionCommand;
		private DelegateCommand _addNewVersioCommand;
		private DelegateCommand _changeVersionStartDateCommand;
		
		public VatRateNomenclatureVersionViewModel(
			Nomenclature entity, 
			IVatRateVersionController  vatRateVersionController,
			ICommonServices commonServices, 
			ViewModelEEVMBuilder<VatRate> vatRateEevmBuilder,
			DialogViewModelBase parentDialog,
			bool isEditable = true) : base(entity, commonServices)
		{
			_vatRateVersionController = vatRateVersionController;
			_vatRateEEVMBuilder = vatRateEevmBuilder;
			_parentDialog = parentDialog;
			_isEditable = isEditable;

			Initialize();
		}

		public IEntityEntryViewModel VatRateEntryViewModel { get; private set; }

		public bool IsEditAvailable => SelectedVatRateVersion != null;
		public bool IsNewOrganization => Entity.Id == 0;
		public bool IsButtonsAvailable { get; }
		
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
		
		public virtual VatRateVersion SelectedVatRateVersion
		{
			get => _selectedVatRateVersion;
			set
			{
				if(SetField(ref _selectedVatRateVersion, value))
				{
					OnPropertyChanged(nameof(CanChangeVersionDate));
					OnPropertyChanged(nameof(IsEditAvailable));
				}
			}
		}
		
		public bool CanAddNewVersion =>
			SelectedDate.HasValue
			&& Entity.VatRateVersions.All(x => x.Id != 0)
			&& _vatRateVersionController.IsValidDateForNewVatRateVersion(SelectedDate.Value);
		
		public bool CanChangeVersionDate =>
			SelectedDate.HasValue
			&& _vatRateVersionController.IsValidDateForVersionStartDateChange(SelectedVatRateVersion, SelectedDate.Value);
		
		#region Commands

		public DelegateCommand SaveEditingVersionCommand =>
			_saveEditingVersionCommand ?? (_saveEditingVersionCommand = new DelegateCommand(() =>
				{
					var version = new VatRateVersion()
					{

					};

					var validationContext = new ValidationContext(version);
					if(!CommonServices.ValidationService.Validate(version, validationContext))
					{
						return;
					}
					
					IsEditVisible = false;
				},
				() => IsEditAvailable
			));

		public DelegateCommand CancelEditingVersionCommand =>
			_cancelEditingVersionCommand ?? (_cancelEditingVersionCommand = new DelegateCommand(() =>
			{
				IsEditVisible = false;
			},
				() => true
			));

		public DelegateCommand EditVersionCommand =>
			_editVersionCommand ?? (_editVersionCommand = new DelegateCommand(() =>
			{
				
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

				SelectedVatRateVersion = _vatRateVersionController.CreateAndAddVersion(SelectedDate);

				EditVersionCommand.Execute();

				OnPropertyChanged(nameof(CanAddNewVersion));
				OnPropertyChanged(nameof(CanChangeVersionDate));
			},
				() => true
			));

		public DelegateCommand ChangeVersionStartDateCommand =>
			_changeVersionStartDateCommand ?? (_changeVersionStartDateCommand = new DelegateCommand(() =>
			{
				if(SelectedDate == null || SelectedVatRateVersion == null)
				{
					return;
				}
				_vatRateVersionController.ChangeVersionStartDate(SelectedVatRateVersion, SelectedDate.Value);

				OnPropertyChanged(nameof(CanAddNewVersion));
				OnPropertyChanged(nameof(CanChangeVersionDate));
			},
				() => IsEditAvailable
			));

		#endregion

		private void Initialize()
		{
			CreateVatRateEEVM();
		}

		private void CreateVatRateEEVM()
		{
			VatRateEntryViewModel = _vatRateEEVMBuilder
					.SetViewModel(_parentDialog)
					.SetUnitOfWork(UoW)
					.ForProperty(Entity, x => x.VatRate)
					.UseViewModelJournalAndAutocompleter<VatRateJournalViewModel>()
					.UseViewModelDialog<VatRateViewModel>()
					.Finish();
		}
	}
}
