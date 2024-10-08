using CustomerAppsApi.Library.Dto;
using Vodovoz.Errors;

namespace CustomerAppsApi.Library.Models
{
	public interface ISendingService
	{
		Result SendCodeToEmail(SendingCodeToEmailDto codeToEmailDto);
	}
}
