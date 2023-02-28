using RevenueService.Client.Enums;
using System;
using Dadata.Model;

namespace RevenueService.Client.Parsers
{
	public class PersonTypeParser
	{
		public PersonType Parse(PartyType partyType)
		{
			switch(partyType)
			{
				case PartyType.LEGAL:
					return PersonType.Legal;
				case PartyType.INDIVIDUAL:
					return PersonType.Individual;
				default:
					throw new NotSupportedException($"{partyType} не поддерживается");
			}
		}
	}
}
