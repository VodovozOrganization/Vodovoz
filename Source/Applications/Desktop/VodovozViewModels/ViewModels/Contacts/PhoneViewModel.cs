using QS.Services;
using QS.ViewModels;
using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using Vodovoz.Domain.Contacts;
using Vodovoz.Settings.Contacts;

namespace Vodovoz.ViewModels.ViewModels.Contacts
{
	public class PhoneViewModel : WidgetViewModelBase
	{
		private readonly IUnitOfWork _uow;
		private readonly bool _canArchiveNumber;
		private readonly IPhoneTypeSettings _phoneTypeSettings;
		private readonly IExternalCounterpartyController _externalCounterpartyController;
		private readonly ICommonServices _commonServices;
		private Phone _phone;

		public PhoneViewModel(
			IUnitOfWork uow,
			ICommonServices commonServices,
			IPhoneTypeSettings phoneTypeSettings,
			IExternalCounterpartyController externalCounterpartyController)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));

			_phoneTypeSettings = phoneTypeSettings ?? throw new ArgumentNullException(nameof(phoneTypeSettings));
			_externalCounterpartyController =
				externalCounterpartyController ?? throw new ArgumentNullException(nameof(externalCounterpartyController));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_canArchiveNumber = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Phone)).CanUpdate;
		}
		
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

		public event Action UpdateExternalCounterpartyAction;

		public Phone GetPhone() => _phone;
		
		public void SetPhone(Phone phone)
		{
			_phone = phone;
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
				_externalCounterpartyController.HasActiveExternalCounterparties(_uow, _phone.Id, out var externalCounterparties);

				var question = externalCounterparties.Any()
					? _externalCounterpartyController.PhoneAssignedExternalCounterpartyMessage + "Вы действительно хотите его заархивировать?"
					: "Номер будет переведен в архив и пропадет в списке активных. Продолжить?";
				
				if(_canArchiveNumber && !_commonServices.InteractiveService.Question(question))
				{
					return false;
				}

				_externalCounterpartyController.DeleteExternalCounterparties(_uow, externalCounterparties);
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
	}
}
