using System;
using System.Collections.Generic;
using Vodovoz.Settings.Organizations;

namespace SmsPaymentService
{
	public class SmsPaymentValidator : ISmsPaymentValidator
	{
		private readonly IList<int> _allowedOrganizationIds;
		private readonly IOrganizationSettings _organizationSettings;

		public SmsPaymentValidator(IOrganizationSettings organizationSettings)
		{
			if(organizationSettings is null)
			{
				throw new ArgumentNullException(nameof(organizationSettings));
			}

			_allowedOrganizationIds = new List<int>
			{
				_organizationSettings.VodovozSouthOrganizationId,
				_organizationSettings.VodovozNorthOrganizationId
			};
		}

		public bool Validate(SmsPaymentDTO smsPaymentDTO, out IList<string> errorMessages)
		{
			errorMessages = new List<string>();
			if(!_allowedOrganizationIds.Contains(smsPaymentDTO.OrganizationId))
			{
				errorMessages.Add($"Для заказа автоматически подобралась невалидная организация (Код: {smsPaymentDTO.OrganizationId})");
				return false;
			}
			return true;
		}
	}
}
