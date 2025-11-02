using QS.Services;
using QS.ViewModels;
using System;
using Vodovoz.Domain.Contacts;
using Vodovoz.Settings.Contacts;
using VodovozBusiness.Domain.Contacts;

namespace Vodovoz.ViewModels.ViewModels.Contacts
{
	public class PhoneViewModel : WidgetViewModelBase
	{
		private Phone _phone;
		private bool _canArchivateNumber;
		private readonly IPhoneTypeSettings _phoneTypeSettings;
		private ICommonServices _commonServices;

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

		public PhoneViewModel(Phone Phone,
			ICommonServices commonServices,
			IPhoneTypeSettings phoneTypeSettings)
		{
			_phone = Phone;

			_phoneTypeSettings = phoneTypeSettings ?? throw new ArgumentNullException(nameof(phoneTypeSettings));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_canArchivateNumber = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Phone)).CanUpdate;
		}

		private void SetPhoneType(PhoneType phoneType)
		{
			if(phoneType.Id == _phoneTypeSettings.ArchiveId)
			{
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
					PhoneIsArchive = false;
				}
			}
			_phone.PhoneType = phoneType;
		}
	}
}
