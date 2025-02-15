using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Controllers;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Settings.Contacts;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;

namespace Vodovoz.ViewModels.ViewModels.Contacts
{
	public class PhoneViewModel : WidgetViewModelBase, IDisposable
	{
		private readonly bool _canArchiveNumber;
		private readonly IPhoneTypeSettings _phoneTypeSettings;
		private readonly IExternalCounterpartyController _externalCounterpartyController;
		private readonly ViewModelEEVMBuilder<RoboAtsCounterpartyName> _roboatsClientNameEevmBuilder;
		private readonly ViewModelEEVMBuilder<RoboAtsCounterpartyPatronymic> _roboatsClientPatronymicEevmBuilder;
		private readonly ICommonServices _commonServices;
		private Phone _phone;
		private RoboAtsCounterpartyName _selectedRoboatsCounterpartyName;
		private RoboAtsCounterpartyPatronymic _selectedRoboatsCounterpartyPatronymic;
		private DialogTabViewModelBase _parentViewModel;

		public PhoneViewModel(
			IUnitOfWork uow,
			ILifetimeScope lifetimeScope,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IPhoneTypeSettings phoneTypeSettings,
			IExternalCounterpartyController externalCounterpartyController,
			ViewModelEEVMBuilder<RoboAtsCounterpartyName> roboatsClientNameEevmBuilder,
			ViewModelEEVMBuilder<RoboAtsCounterpartyPatronymic> roboatsClientPatronymicEevmBuilder)
		{
			UoW = uow ?? throw new ArgumentNullException(nameof(uow));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

			_phoneTypeSettings = phoneTypeSettings ?? throw new ArgumentNullException(nameof(phoneTypeSettings));
			_externalCounterpartyController =
				externalCounterpartyController ?? throw new ArgumentNullException(nameof(externalCounterpartyController));
			_roboatsClientNameEevmBuilder =
				roboatsClientNameEevmBuilder ?? throw new ArgumentNullException(nameof(roboatsClientNameEevmBuilder));
			_roboatsClientPatronymicEevmBuilder =
				roboatsClientPatronymicEevmBuilder ?? throw new ArgumentNullException(nameof(roboatsClientPatronymicEevmBuilder));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_canArchiveNumber = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Phone)).CanUpdate;
		}
		
		public IUnitOfWork UoW { get; }
		public ILifetimeScope LifetimeScope { get; private set; }
		public INavigationManager NavigationManager { get; }

		public PhoneType SelectedPhoneType
		{
			get => _phone.PhoneType;
			set => SetPhoneType(value);
		}
		
		public bool PhoneIsArchive
		{
			get => _phone.IsArchive;
			set => _phone.IsArchive = value;
		}

		public RoboAtsCounterpartyName SelectedRoboatsCounterpartyName
		{
			get => _selectedRoboatsCounterpartyName;
			set => SetField(ref _selectedRoboatsCounterpartyName, value);
		}

		public RoboAtsCounterpartyPatronymic SelectedRoboatsCounterpartyPatronymic
		{
			get => _selectedRoboatsCounterpartyPatronymic;
			set => SetField(ref _selectedRoboatsCounterpartyPatronymic, value);
		}

		public event Action UpdateExternalCounterpartyAction;

		public Phone Phone
		{
			get => _phone;
			private set => SetField(ref _phone, value);
		}
		
		public ITdiTab ParentTab { get; private set; }
		public IEntityEntryViewModel RoboatsCounterpartyNameViewModel { get; private set; }
		public IEntityEntryViewModel RoboatsCounterpartyPatronymicViewModel { get; private set; }
		
		public void Initialize(Phone phone, ITdiTab parentTab = null)
		{
			SetPhone(phone);
			ParentTab = parentTab;
		}
		
		public void Initialize(Phone phone, DialogTabViewModelBase parentViewModel = null)
		{
			SetPhone(phone);
			_parentViewModel = parentViewModel;
			InitializeRoboatsViewModels();
		}

		private void SetPhone(Phone phone)
		{
			Phone = phone;
		}

		private void InitializeRoboatsViewModels()
		{
			if(_parentViewModel is null)
			{
				return;
			}
			
			var nameViewModel =
				_roboatsClientNameEevmBuilder
					.SetUnitOfWork(UoW)
					.SetViewModel(_parentViewModel)
					.ForProperty(Phone, x => x.RoboAtsCounterpartyName)
					.UseViewModelDialog<RoboAtsCounterpartyNameViewModel>()
					.UseViewModelJournalAndAutocompleter<RoboAtsCounterpartyNameJournalViewModel>()
					.Finish();
			
			RoboatsCounterpartyNameViewModel = nameViewModel;
			
			var patronymicViewModel =
				_roboatsClientPatronymicEevmBuilder
					.SetUnitOfWork(UoW)
					.SetViewModel(_parentViewModel)
					.ForProperty(Phone, x => x.RoboAtsCounterpartyPatronymic)
					.UseViewModelDialog<RoboAtsCounterpartyNameViewModel>()
					.UseViewModelJournalAndAutocompleter<RoboAtsCounterpartyNameJournalViewModel>()
					.Finish();

			RoboatsCounterpartyPatronymicViewModel = patronymicViewModel;
		}

		private void SetPhoneType(PhoneType phoneType)
		{
			var result = _phone.Counterparty != null
				? SetPhoneTypeToCounterpartyPhone(phoneType)
				: DefaultSetPhoneType(phoneType);

			if(result)
			{
				_phone.PhoneType = phoneType;
			}
			
			OnPropertyChanged(nameof(SelectedPhoneType));
		}

		private bool SetPhoneTypeToCounterpartyPhone(PhoneType phoneType)
		{
			if(phoneType.Id == _phoneTypeSettings.ArchiveId)
			{
				_externalCounterpartyController.HasActiveExternalCounterparties(UoW, _phone.Id, out var externalCounterparties);

				var question = externalCounterparties.Any()
					? _externalCounterpartyController.PhoneAssignedExternalCounterpartyMessage + "Вы действительно хотите его заархивировать?"
					: "Номер будет переведен в архив и пропадет в списке активных. Продолжить?";
				
				if(_canArchiveNumber && !_commonServices.InteractiveService.Question(question))
				{
					return false;
				}

				_externalCounterpartyController.DeleteExternalCounterparties(UoW, externalCounterparties);
				PhoneIsArchive = true;
				UpdateExternalCounterpartyAction?.Invoke();
			}
			else
			{
				PhoneIsArchive = false;
			}

			return true;
		}
		
		private bool DefaultSetPhoneType(PhoneType phoneType)
		{
			if(phoneType.Id == _phoneTypeSettings.ArchiveId)
			{
				var question = "Номер будет переведен в архив и пропадет в списке активных. Продолжить?";
				
				if(_canArchiveNumber && !_commonServices.InteractiveService.Question(question))
				{
					return false;
				}
				
				PhoneIsArchive = true;
			}
			else
			{
				PhoneIsArchive = false;
			}

			return true;
		}

		public void Dispose()
		{
			_parentViewModel = null;
			ParentTab = null;
			LifetimeScope = null;
		}
	}
}
