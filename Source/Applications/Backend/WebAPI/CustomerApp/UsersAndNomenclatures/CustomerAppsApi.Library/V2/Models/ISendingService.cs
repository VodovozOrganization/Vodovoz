using CustomerAppsApi.Library.V2.Dto;
using Vodovoz.Core.Domain.Results;

namespace CustomerAppsApi.Library.V2.Models
{
	public interface ISendingService
	{
		Result SendCodeToEmail(SendingCodeToEmailDto codeToEmailDto, bool isDryRun = false);
	}
}
