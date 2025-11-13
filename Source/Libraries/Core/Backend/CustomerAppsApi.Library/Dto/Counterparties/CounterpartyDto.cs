using System;
using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Data.Interfaces.Counterparties;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	public class CounterpartyDto : ExternalCounterpartyDto, ILegalCounterpartyInfo
	{
		public string Name { get; set; }
		public string FullName { get; set; }
		public int ErpCounterpartyId { get; set; }
		public string FirstName { get; set; }
		public string Surname { get; set; }
		public string Patronymic { get; set; }
		public string PhoneNumber { get; set; }
		public string Email { get; set; }

		[JsonConverter(typeof(JsonStringEnumConverter))]
		public PersonType PersonType { get; set; }
		public string Inn { get; set; }
		public string Kpp { get; set; }
		public string ShortTypeOfOwnership { get; set; }

		[JsonConverter(typeof(JsonStringEnumConverter))]
		public TaxType? TaxType { get; set; }
		public int CameFromId { get; set; }
		public string JurAddress { get; set; }
	}
}
