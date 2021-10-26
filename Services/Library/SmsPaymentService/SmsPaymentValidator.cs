using System.Collections.Generic;
using Vodovoz.Services;

namespace SmsPaymentService
{
	public class SmsPaymentValidator : ISmsPaymentValidator
	{
		private readonly IList<int> _allowedOrganizationIds;

		public SmsPaymentValidator(IOrganizationParametersProvider organizationParametersProvider)
		{
			_allowedOrganizationIds = new List<int>
			{
				organizationParametersProvider.VodovozSouthOrganizationId,
				organizationParametersProvider.VodovozNorthOrganizationId
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
