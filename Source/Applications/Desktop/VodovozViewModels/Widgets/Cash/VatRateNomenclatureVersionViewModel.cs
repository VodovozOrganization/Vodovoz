using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
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
		private readonly ViewModelEEVMBuilder<VatRate> _vatRateEevmBuilder;
		private readonly DialogViewModelBase _parentDialog;
		private readonly IUnitOfWork _uow;
		private readonly bool _isEditable;

		private DateTime? _selectedDate;
		private VatRateVersion _selectedVatRateVersion;
		private bool _isEditVisible;
		private DelegateCommand _saveEditingVersionCommand;
		private DelegateCommand _cancelEditingVersionCommand;
		private DelegateCommand _editVersionCommand;
		private DelegateCommand _addNewVersioCommand;
		private DelegateCommand _changeVersionStartDateCommand;
		private VatRate _selectedVatRate;

		public VatRateNomenclatureVersionViewModel(
			Nomenclature entity, 
			IVatRateVersionController  vatRateVersionController,
			ICommonServices commonServices, 
			ViewModelEEVMBuilder<VatRate> vatRateEevmBuilder,
			DialogViewModelBase parentDialog,
			IUnitOfWork uow,
			bool isEditable = true) : base(entity, commonServices)
		{
			_vatRateVersionController = vatRateVersionController;
			_vatRateEevmBuilder = vatRateEevmBuilder;
			_parentDialog = parentDialog;
			_uow = uow;
			_isEditable = isEditable;

			Initialize();
			
			IsButtonsAvailable = isEditable;
		}
		
		public bool IsEditAvailable => SelectedVatRateVersion != null;
		public bool IsNewOrganization => Entity.Id == 0;
		public bool IsButtonsAvailable { get; }
		
		public IEntityEntryViewModel VatRateEntryViewModel { get; private set; }
		
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

		public virtual VatRate SelectedVatRate
		{
			get => _selectedVatRate;
			set {
				if(SetField(ref _selectedVatRate, value))
				{
					OnPropertyChanged(nameof(CanSaveSelectedVatRate));
				} 
			}
		}

		public bool CanAddNewVersion =>
			SelectedDate.HasValue
			&& Entity.VatRateVersions.All(x => x.Id != 0)
			&& _vatRateVersionController.IsValidDateForNewVatRateVersion(SelectedDate.Value, VatRateVersionType.Nomenclature);
		
		public bool CanChangeVersionDate =>
			SelectedDate.HasValue
			&& _vatRateVersionController.IsValidDateForVersionStartDateChange(SelectedVatRateVersion, SelectedDate.Value, VatRateVersionType.Nomenclature);
		
		public bool CanSaveSelectedVatRate => SelectedVatRate != null;
		
		#region Commands

		public DelegateCommand SaveEditingVersionCommand =>
			_saveEditingVersionCommand ?? (_saveEditingVersionCommand = new DelegateCommand(() =>
				{
					var version = new VatRateVersion()
					{
						VatRate = SelectedVatRate,
					};

					var validationContext = new ValidationContext(version);
					if(!CommonServices.ValidationService.Validate(version, validationContext))
					{
						return;
					}
					
					SelectedVatRateVersion.VatRate = SelectedVatRate;
					
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

				SelectedVatRateVersion = _vatRateVersionController.CreateAndAddVersion(VatRateVersionType.Nomenclature, SelectedDate);

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
				_vatRateVersionController.ChangeVersionStartDate(SelectedVatRateVersion, SelectedDate.Value, VatRateVersionType.Nomenclature);

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
			VatRateEntryViewModel = _vatRateEevmBuilder
				.SetViewModel(_parentDialog)
				.SetUnitOfWork(_uow)
				.ForProperty(this, x => x.SelectedVatRate)
				.UseViewModelJournalAndAutocompleter<VatRateJournalViewModel>()
				.UseViewModelDialog<VatRateViewModel>()
				.Finish();
		}

		private void ClearProperties()
		{
			SelectedVatRate = null;
		}
	}
}
