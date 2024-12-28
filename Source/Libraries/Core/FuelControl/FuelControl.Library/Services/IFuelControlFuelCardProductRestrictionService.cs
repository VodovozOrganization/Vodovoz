using FuelControl.Contracts.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FuelControl.Library.Services
{
	public interface IFuelControlFuelCardProductRestrictionService
	{
		Task<IEnumerable<FuelCardProductRestrictionDto>> GetProductRestrictionsByCardId(string cardId, string sessionId, string apiKey, CancellationToken cancellationToken);
		Task<bool> RemoveProductRestictionById(string restrictionId, string sessionId, string apiKey, CancellationToken cancellationToken);
		Task<IEnumerable<string>> SetProductRestriction(string cardId, string productGroupId, string sessionId, string apiKey, CancellationToken cancellationToken);
	}
}