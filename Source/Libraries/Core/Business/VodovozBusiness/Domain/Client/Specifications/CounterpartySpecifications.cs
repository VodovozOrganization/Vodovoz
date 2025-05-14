using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Client.Specifications
{
	/// <summary>
	/// Спецификации для фильтрации контрагентов
	/// </summary>
	public static class CounterpartySpecifications
	{
		/// <summary>
		/// Спецификация для фильтрации контрагентов по идентификатору
		/// </summary>
		/// <param name="counterpartyId">Идентификатор контрагента</param>
		/// <returns></returns>
		public static ExpressionSpecification<Counterparty> CreateForId(int counterpartyId)
			=> new ExpressionSpecification<Counterparty>(x => x.Id == counterpartyId);
	}
}
