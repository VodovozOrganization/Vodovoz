using QS.Services;
using QS.ViewModels;
using System;
using QS.Dialog;
using QS.DomainModel.UoW;
using Vodovoz.Application.Clients;
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
			IExternalCounterpartyHandler externalCounterpartyHandler)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_phone = phone;

			_phoneTypeSettings = phoneTypeSettings ?? throw new ArgumentNullException(nameof(phoneTypeSettings));
			_externalCounterpartyHandler = externalCounterpartyHandler ?? throw new ArgumentNullException(nameof(externalCounterpartyHandler));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_canArchivateNumber = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Phone)).CanUpdate;
		}

		private void SetPhoneType(PhoneType phoneType)
		{
			if(phoneType.Id == _phoneTypeSettings.ArchiveId)
			{
				if(_externalCounterpartyHandler.HasExternalCounterparties(_uow, _phone))
				{
					_commonServices.InteractiveService
						.ShowMessage(
							ImportanceLevel.Warning,
							"По данному номеру привязан пользователь ИПЗ(МП, сайта и т.д.) архивация/деархивация невозможна. Обратитесь в отдель разработки");
				
					//_phone.PhoneType = _phone.PhoneType;
					return;
				}
				
				if(_canArchivateNumber && !_commonServices.InteractiveService.Question("Номер будет переведен в архив и пропадет в списке активных. Продолжить?"))
				{
					return;
				}

				PhoneIsArchive = true;
			}
			else
			{
				if(PhoneIsArchive)
				{
					if(_externalCounterpartyHandler.HasExternalCounterparties(_uow, _phone))
					{
						_commonServices.InteractiveService
							.ShowMessage(
								ImportanceLevel.Warning,
								"По данному номеру привязан пользователь ИПЗ(МП, сайта и т.д.) архивация/деархивация невозможна. Обратитесь в отдель разработки");
				
						//_phone.PhoneType = _phone.PhoneType;
						return;
					}
					
					PhoneIsArchive = false;
				}
			}
			
			_phone.PhoneType = phoneType;
		}
	}
}
