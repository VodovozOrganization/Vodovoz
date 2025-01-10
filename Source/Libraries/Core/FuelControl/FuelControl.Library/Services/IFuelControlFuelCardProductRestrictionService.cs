using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FuelControl.Library.Services
{
	public interface IFuelControlFuelCardProductRestrictionService
	{
		Task<IEnumerable<string>> GetProductRestrictionsByCardId(string cardId, string sessionId, string apiKey, CancellationToken cancellationToken);
		Task<bool> RemoveProductRestictionById(string restrictionId, string sessionId, string apiKey, CancellationToken cancellationToken);
		Task<IEnumerable<long>> SetCommonFuelRestriction(string cardId, string sessionId, string apiKey, CancellationToken cancellationToken);
		Task<IEnumerable<long>> SetFuelProductGroupRestriction(string cardId, string productGroupId, string sessionId, string apiKey, CancellationToken cancellationToken);
	}
}
