using System.Threading.Tasks;
using CustomerAppsApi.Library.Dto;
using Vodovoz.Domain.Client;

namespace ExternalCounterpartyAssignNotifier.Services
{
	public interface INotificationService
	{
		Task<int> NotifyOfCounterpartyAssignAsync(
			RegisteredNaturalCounterpartyDto counterpartyDto, CounterpartyFrom counterpartyFrom);
	}
}
