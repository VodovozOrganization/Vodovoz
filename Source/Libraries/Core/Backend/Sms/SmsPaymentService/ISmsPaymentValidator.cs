using System.Collections.Generic;

namespace SmsPaymentService
{
	public interface ISmsPaymentValidator
	{
		bool Validate(SmsPaymentDTO smsPaymentDTO, out IList<string> errorMessages);
	}
}
