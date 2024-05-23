using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public class ExistingExternalCounterpartyNode : ICounterpartyWithPhoneNode
	{
		public int EntityId { get; set; }
		public Guid ExternalCounterpartyGuid { get; set; }
		public int ExternalCounterpartyId { get; set; }
		public int? PhoneId { get; set; }
		public PersonType PersonType { get; set; }
		public string CounterpartyName { get; set; }
		public string PhoneNumber { get; set; }
		public DateTime? LastOrderDate { get; set; }
		public string PersonTypeShort
		{
			get
			{
				switch(PersonType)
				{
					case PersonType.legal:
						return "ЮЛ";
					case PersonType.natural:
						return "ФЛ";
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}
