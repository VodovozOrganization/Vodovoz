using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;

namespace Vodovoz.ViewModels.ViewModels.Counterparty
{
	public class DeliveryPointMatchingNode : IExternalCounterpartyMatchingNode
	{
		public int EntityId { get; private set; }
		public int? ExternalCounterpartyId { get; }
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
		public string Title { get; private set; }
		public bool Matching { get; private set; }
		private DateTime? LastOrderDate { get; set; }
		public string LastOrderDateString => LastOrderDate.HasValue ? LastOrderDate.Value.ToShortDateString() : "-";
		public bool HasOtherExternalCounterparty { get; }
		public CounterpartyMatchingNode CounterpartyMatchingNode { get; private set; }

		public static DeliveryPointMatchingNode Create(
			int entityId, PersonType personType, DateTime? lastOrderDate, CounterpartyMatchingNode counterpartyNode, string title)
		{
			return new DeliveryPointMatchingNode
			{
				EntityId = entityId,
				PersonType = personType,
				LastOrderDate = lastOrderDate,
				CounterpartyMatchingNode = counterpartyNode,
				Title = title,
				Matching = true
			};
		}
	}
}
