using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.ViewModels.Contacts
{
	public class PhoneViewModel : WidgetViewModelBase
	{
		private readonly Phone _phone;
		private readonly IUnitOfWork _uow;
		private readonly bool _canArchiveNumber;
		private readonly IPhoneTypeSettings _phoneTypeSettings;
		private readonly IExternalCounterpartyController _externalCounterpartyController;
		private readonly ICommonServices _commonServices;
		private IList<ExternalCounterparty> _externalCounterparties;

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
			_canArchiveNumber = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Phone)).CanUpdate;
		}

		private void SetPhoneType(PhoneType phoneType)
		{
			if(_phone.Counterparty != null)
			{
				SetPhoneTypeToCounterpartyPhone(phoneType);
			}
			else
			{
				DefaultSetPhoneType(phoneType);
			}
			
			_phone.PhoneType = phoneType;
		}

		private void SetPhoneTypeToCounterpartyPhone(PhoneType phoneType)
		{
			if(phoneType.Id == _phoneTypeSettings.ArchiveId)
			{
				_externalCounterpartyController.HasActiveExternalCounterparties(_uow, _phone.Id, out var externalCounterparties);

				var question = externalCounterparties.Any()
					? "Данный номер телефона привязан к внешнему пользователю сайта/приложения\n" +
					"Вы действительно хотите его заархивировать?"
					: "Номер будет переведен в архив и пропадет в списке активных. Продолжить?";
				
				if(_canArchiveNumber && !_commonServices.InteractiveService.Question(question))
				{
					return;
				}

				_externalCounterparties = externalCounterparties;
				_externalCounterpartyController.ArchiveExternalCounterparties(_externalCounterparties);
				PhoneIsArchive = true;
			}
			else
			{
				if(!PhoneIsArchive)
				{
					return;
				}

				PhoneIsArchive = false;

				if(_externalCounterparties != null && _externalCounterparties.Any())
				{
					_externalCounterpartyController.UndoArchiveExternalCounterparties(_externalCounterparties);
				}
			}
		}
		
		private void DefaultSetPhoneType(PhoneType phoneType)
		{
			if(phoneType.Id == _phoneTypeSettings.ArchiveId)
			{
				var question = "Номер будет переведен в архив и пропадет в списке активных. Продолжить?";
				
				if(_canArchiveNumber && !_commonServices.InteractiveService.Question(question))
				{
					return;
				}
				
				PhoneIsArchive = true;
			}
			else
			{
				if(PhoneIsArchive)
				{
					PhoneIsArchive = false;
				}
			}
		}
	}
}
