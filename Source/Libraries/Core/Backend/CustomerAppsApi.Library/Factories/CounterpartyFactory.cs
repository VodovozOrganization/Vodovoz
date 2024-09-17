using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Factories
{
	public class CounterpartyFactory : ICounterpartyFactory
	{
		public Counterparty CreateCounterpartyFromExternalSource(CounterpartyDto counterpartyDto)
		{
			switch(counterpartyDto.PersonType)
			{
				case PersonType.legal:
					return CreateLegalCounterparty(counterpartyDto);
				default:
					return CreateNaturalCounterparty(counterpartyDto);
			}
		}
		
		public CounterpartyBottlesDebtDto CounterpartyBottlesDebtDto(int counterpartyId, int debt)
		{
			return new CounterpartyBottlesDebtDto
			{
				ErpCounterpartyId = counterpartyId,
				CounterpartyBottlesDebt = debt
			};
		}

		private Counterparty CreateLegalCounterparty(CounterpartyDto counterpartyDto)
		{
			return new Counterparty
			{
				Name = counterpartyDto.Name,
				FullName = counterpartyDto.FullName ?? counterpartyDto.Name,
				TypeOfOwnership = counterpartyDto.TypeOfOwnership,
				TaxType = counterpartyDto.TaxType.Value,
				INN = counterpartyDto.Inn,
				KPP = counterpartyDto.Kpp,
				JurAddress = counterpartyDto.JurAddress,
			};
		}
		
		private Counterparty CreateNaturalCounterparty(CounterpartyDto counterpartyDto)
		{
			return new Counterparty
			{
				FirstName = counterpartyDto.FirstName,
				Surname = counterpartyDto.Surname,
				Patronymic = counterpartyDto.Patronymic,
				Name = $"{counterpartyDto.Surname} {counterpartyDto.FirstName} {counterpartyDto.Patronymic}"
			};
		}
	}
}
