using System;
using TaxcomEdo.Contracts.Counterparties;
using Vodovoz.Core.Domain.Clients;

namespace VodovozBusiness.Converters
{
	public class PersonTypeConverter : IPersonTypeConverter
	{
		public CounterpartyInfoType ConvertPersonTypeToCounterpartyInfoType(PersonType personType)
		{
			switch(personType)
			{
				case PersonType.legal:
					return CounterpartyInfoType.Legal;
				case PersonType.natural:
					return CounterpartyInfoType.Natural;
				default:
					throw new ArgumentOutOfRangeException(nameof(personType), personType, null);
			}
		}
	}
}
