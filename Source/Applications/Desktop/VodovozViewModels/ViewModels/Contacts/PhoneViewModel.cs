using QS.Services;
using QS.ViewModels;
using System;
using QS.Dialog;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Domain.Contacts;
using Vodovoz.Settings.Contacts;
using VodovozBusiness.Domain.Contacts;
using VodovozBusiness.Services.Clients;

namespace Vodovoz.ViewModels.ViewModels.Contacts
{
	public class PhoneViewModel : WidgetViewModelBase
	{
		private readonly IUnitOfWork _uow;
		private readonly Phone _phone;
		private readonly bool _canArchivateNumber;
		private readonly bool _supportsExternalCounterpartyArchiving;
		private bool _wasInitiallyArchive;
		private readonly IPhoneTypeSettings _phoneTypeSettings;
		private readonly IExternalCounterpartyHandler _externalCounterpartyHandler;
		private ICommonServices _commonServices;

		public PhoneType SelectedPhoneType
		{
			get => _phone.PhoneType;
			set
			{
				SetPhoneType(value);
				OnPropertyChanged();
			}
		}

		public bool PhoneIsArchive
		{
			get => _phone.IsArchive;
			set => _phone.IsArchive = value;
		}

		public PhoneViewModel(
			IUnitOfWork uow,
			Phone phone,
			ICommonServices commonServices,
			IPhoneTypeSettings phoneTypeSettings,
			IExternalCounterpartyHandler externalCounterpartyHandler,
			bool supportsExternalCounterpartyArchiving = false)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_phone = phone;
			_wasInitiallyArchive = phone.IsArchive;
			_supportsExternalCounterpartyArchiving = supportsExternalCounterpartyArchiving;

			_phoneTypeSettings = phoneTypeSettings ?? throw new ArgumentNullException(nameof(phoneTypeSettings));
			_externalCounterpartyHandler = externalCounterpartyHandler ?? throw new ArgumentNullException(nameof(externalCounterpartyHandler));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_canArchivateNumber = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Phone)).CanUpdate;
		}

		/// <summary>Телефон переведён в архив после загрузки или последнего сохранения.</summary>
		public bool IsPendingArchiving => !_wasInitiallyArchive && PhoneIsArchive;

		/// <summary>Запомнить состояние телефона после успешного сохранения карточки.</summary>
		public void AcceptChanges()
		{
			_wasInitiallyArchive = PhoneIsArchive;
		}

		private void SetPhoneType(PhoneType phoneType)
		{
			if(phoneType == null || phoneType == _phone.PhoneType)
			{
				return;
			}

			if(phoneType.Id == _phoneTypeSettings.ArchiveId)
			{
				var hasExternalCounterparties = _externalCounterpartyHandler.HasExternalCounterparties(_uow, _phone);
				if(hasExternalCounterparties
					&& (!_supportsExternalCounterpartyArchiving || !_canArchivateNumber
						|| !_commonServices.CurrentPermissionService.ValidatePresetPermission(
							CounterpartyPermissions.CanArchivePhoneWithExternalCounterparties)))
				{
					_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
						"Для архивации телефона, привязанного к пользователям ИПЗ, требуется специальное право и право редактирования телефона.");
					return;
				}

				var confirmation = hasExternalCounterparties
					? "Номер будет переведен в архив. При сохранении карточки будут удалены все привязанные пользователи ИПЗ, связанные заявки и уведомления о сопоставлении. Продолжить?"
					: "Номер будет переведен в архив и пропадет в списке активных. Продолжить?";
				if(_canArchivateNumber && !_commonServices.InteractiveService.Question(confirmation))
				{
					return;
				}

				PhoneIsArchive = true;
			}
			else if(PhoneIsArchive)
			{
				// Возврат исходного типа до сохранения отменяет запланированную архивацию.
				if(!IsPendingArchiving && CheckExternalCounterparties())
				{
					return;
				}

				PhoneIsArchive = false;
			}

			_phone.PhoneType = phoneType;
		}

		private bool CheckExternalCounterparties()
		{
			if(_externalCounterpartyHandler.HasExternalCounterparties(_uow, _phone))
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
					"По данному номеру привязан пользователь ИПЗ. Деархивация невозможна. Обратитесь в отдел разработки.");
				return true;
			}

			return false;
		}
	}
}
