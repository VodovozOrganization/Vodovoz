using CustomerAppsApi.Library.V1.Dto;
using Vodovoz.Core.Domain.Results;

namespace CustomerAppsApi.Library.V1.Models
{
	public interface ISendingService
	{
		Result SendCodeToEmail(SendingCodeToEmailDto codeToEmailDto, bool isDryRun = false);
	}
}
