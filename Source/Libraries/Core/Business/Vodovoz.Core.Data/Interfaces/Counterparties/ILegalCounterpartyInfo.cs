using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Data.Interfaces.Counterparties
{
	public interface ILegalCounterpartyInfo
	{
		string Name { get; set; }
		string FullName { get; set; }
		string ShortTypeOfOwnership { get; set; }
		TaxType? TaxType { get; set; }
		string Inn { get; set; }
		string Kpp { get; set; }
		string JurAddress { get; set; }
	}
}
