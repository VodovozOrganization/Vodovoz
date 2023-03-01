using RevenueService.Client.Enums;
using System;
using Dadata.Model;

namespace RevenueService.Client.Parsers
{
	public class CounterpartyTypeParser
	{
		public CounterpartyType Parse(PartyType partyType)
		{
			switch(partyType)
			{
				case PartyType.LEGAL:
					return CounterpartyType.Legal;
				case PartyType.INDIVIDUAL:
					return CounterpartyType.Individual;
				default:
					throw new NotSupportedException($"{partyType} не поддерживается");
			}
		}
	}
}
