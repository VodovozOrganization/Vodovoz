using QS.Services;
using QS.ViewModels;
using System;
using QS.Dialog;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using Vodovoz.Domain.Contacts;
using Vodovoz.Settings.Contacts;
using VodovozBusiness.Domain.Contacts;

namespace Vodovoz.ViewModels.ViewModels.Contacts
{
	public class PhoneViewModel : WidgetViewModelBase
	{
		private readonly Phone _phone;
		private readonly IUnitOfWork _uow;
		private readonly bool _canEditType;
		private readonly IPhoneTypeSettings _phoneTypeSettings;
		private readonly IExternalCounterpartyController _externalCounterpartyController;
		private readonly ICommonServices _commonServices;
		private bool _isPhoneNumberEditable;

		public PhoneViewModel(
			Phone phone,
			IUnitOfWork uow,
			ICommonServices commonServices,
			IPhoneTypeSettings phoneTypeSettings,
			IExternalCounterpartyController externalCounterpartyController)
		{
			_phone = phone;
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));

			_phoneTypeSettings = phoneTypeSettings ?? throw new ArgumentNullException(nameof(phoneTypeSettings));
			_externalCounterpartyController =
				externalCounterpartyController ?? throw new ArgumentNullException(nameof(externalCounterpartyController));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_canEditType = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(PhoneType)).CanUpdate;
		}

		public PhoneType SelectedPhoneType
		{
			get => _phone.PhoneType;
			set => UpdatePhoneType(value);
		}
		public bool PhoneIsArchive
		{
			get => _phone.IsArchive;
			set => _phone.IsArchive = value;
		}

		public bool IsPhoneNumberEditable
		{
			get => _isPhoneNumberEditable;
			set => UpdateIsPhoneNumberEditable(value);
		}

		public Phone GetPhone() => _phone;

		private void UpdatePhoneType(PhoneType phoneType)
		{
			if(!_canEditType)
			{
				_commonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Нет прав на редактирование типа телефона");
				return;
			}
			
			var result = SetPhoneType(phoneType);
			
			if(result)
			{
				_phone.PhoneType = phoneType;
			}
			OnPropertyChanged(nameof(SelectedPhoneType));
		}

		private bool SetPhoneType(PhoneType phoneType)
		{
			if(phoneType.Id == _phoneTypeSettings.ArchiveId)
			{
				if(_phone.Counterparty is null)
				{
					var question = "Номер будет переведен в архив и пропадет в списке активных. Продолжить?";
				
					if(!_commonServices.InteractiveService.Question(question))
					{
						return false;
					}
				}
				else
				{
					if(_externalCounterpartyController.CheckActiveExternalCounterparties(_uow, _phone))
					{
						return false;
					}
				}
				
				PhoneIsArchive = true;
			}
			else
			{
				PhoneIsArchive = false;
			}

			return true;
		}
		
		private void UpdateIsPhoneNumberEditable(bool value = true)
		{
			if(!value)
			{
				_isPhoneNumberEditable = false;
			}
			else
			{
				_isPhoneNumberEditable = !_externalCounterpartyController.HasActiveExternalCounterparties(_uow, _phone);
			}
			
			OnPropertyChanged(nameof(IsPhoneNumberEditable));
		}
	}
}
