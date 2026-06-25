using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Factories
{
	public class RegisteredNaturalCounterpartyDtoFactory : IRegisteredNaturalCounterpartyDtoFactory
	{
		public RegisteredNaturalCounterpartyDto CreateNewRegisteredNaturalCounterpartyDto(ExternalCounterparty externalCounterparty)
		{
			return new RegisteredNaturalCounterpartyDto
			{
				ExternalCounterpartyId = externalCounterparty.ExternalCounterpartyId,
				Email = externalCounterparty.Email?.Address,
				ErpCounterpartyId = externalCounterparty.Phone.Counterparty.Id,
				FirstName = externalCounterparty.Phone.Counterparty.FirstName,
				Surname = externalCounterparty.Phone.Counterparty.Surname,
				Patronymic = externalCounterparty.Phone.Counterparty.Patronymic
			};
		}
	}
}
