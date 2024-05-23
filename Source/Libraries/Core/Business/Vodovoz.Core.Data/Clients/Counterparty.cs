using System;
using System.Collections.Generic;
using Vodovoz.Core.Data.Goods;

namespace Vodovoz.Core.Data.Clients
{
	public class Counterparty
	{
		public int Id { get; set; }
		public string PersonalAccountIdInEdo { get; set; }
		public string JurAddress { get; set; }
		public string FullName { get; set; }
		public string INN { get; set; }
		public string KPP { get; set; }
		public bool UseSpecialDocFields { get; set; }
		public string SpecialContractName { get; set; }
		public string SpecialContractNumber { get; set; }
		public DateTime? SpecialContractDate { get; set; }
		public string SpecialCustomer { get; set; }
		public string PayerSpecialKPP { get; set; }
		public string CargoReceiver { get; set; }
		public CargoReceiverSource CargoReceiverSource { get; set; }
		public PersonType PersonType { get; set; }
		public ReasonForLeaving ReasonForLeaving { get; set; }
		public int? CounterpartyExternalOrderId { get; set; }
		public IList<DeliveryPoint> DeliveryPoints { get; set; }
		public IList<SpecialNomenclature> SpecialNomenclatures { get; set; }
	}
}
