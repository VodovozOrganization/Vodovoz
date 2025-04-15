using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Core.Domain.Clients;

namespace VodovozBusiness.Converters
{
	public interface IPersonTypeConverter
	{
		CounterpartyInfoType ConvertPersonTypeToCounterpartyInfoType(PersonType personType);
	}
}
