using Vodovoz.Domain.Contacts;

namespace Vodovoz.Controllers.ContactsForExternalCounterparty
{
	public struct ProcessedContacts
	{
		public int CountCounterparties { get; set; }
		public int CountDeliveryPoints { get; set; }
		public int SumCounterpartiesIds { get; set; }
		public int SumDeliveryPointsCounterpartiesIds { get; set; }
		public Phone CounterpartyPhone { get; set; }
	}
}
