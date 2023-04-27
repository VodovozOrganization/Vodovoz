using Vodovoz.Domain.Contacts;

namespace Vodovoz.Controllers.ContactsForExternalCounterparty
{
	public struct FoundContact
	{
		/// <summary>
		/// Найденный телефон, не null, если FoundContactStatus == Success
		/// в остальных случаях null
		/// </summary>
		public Phone Phone { get; set; }
		public FoundContactStatus FoundContactStatus { get; set; }
	}
}
