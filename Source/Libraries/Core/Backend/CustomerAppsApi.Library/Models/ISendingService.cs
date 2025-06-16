using CustomerAppsApi.Library.Dto;
using Vodovoz.Core.Domain.Results;

namespace CustomerAppsApi.Library.Models
{
	public interface ISendingService
	{
		Result SendCodeToEmail(SendingCodeToEmailDto codeToEmailDto);
	}
}
