using RevenueService.Client.Enums;
using System;
using Dadata.Model;

namespace RevenueService.Client.Parsers
{
	public class TypeOfOwnershipParser
	{
		public TypeOfOwnership Parse(PartyType typeOfOwnership)
		{
			switch(typeOfOwnership)
			{
				case PartyType.LEGAL:
					return TypeOfOwnership.Legal;
				case PartyType.INDIVIDUAL:
					return TypeOfOwnership.Individual;
				default:
					throw new NotSupportedException($"{typeOfOwnership} не поддерживается");
			}
		}
	}
}
