using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public class CounterpartyMatchingNode : IExternalCounterpartyMatchingNode, ICounterpartyWithPhoneNode
	{
		public int EntityId { get; private set; }
		public int? ExternalCounterpartyId { get; set; }
		public PersonType PersonType { get; private set; }

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
		
		public int? PhoneId { get; set; }
		public string Title { get; private set; }
		public bool Matching { get; private set; }
		public bool HasOtherExternalCounterparty { get; set; }
		private DateTime? LastOrderDate { get; set; }
		public string LastOrderDateString => LastOrderDate.HasValue ? LastOrderDate.Value.ToShortDateString() : "-";

		public IList<DeliveryPointMatchingNode> DeliveryPoints { get; set; } = new List<DeliveryPointMatchingNode>();

		public static CounterpartyMatchingNode Create(
			int entityId,
			PersonType personType,
			DateTime? lastOrderDate,
			string title,
			bool matching,
			int? phoneId = null,
			int? externalCounterpartyId = null,
			bool hasOtherExternalCounterparty = false)
		{
			return new CounterpartyMatchingNode
			{
				EntityId = entityId,
				PersonType = personType,
				LastOrderDate = lastOrderDate,
				Title = title,
				PhoneId = phoneId,
				Matching = matching,
				ExternalCounterpartyId = externalCounterpartyId,
				HasOtherExternalCounterparty = hasOtherExternalCounterparty
			};
		}
	}
}
