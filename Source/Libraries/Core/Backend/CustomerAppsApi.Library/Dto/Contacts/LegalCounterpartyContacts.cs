using System.Collections.Generic;

namespace CustomerAppsApi.Library.Dto.Contacts
{
	/// <summary>
	/// Списки контактов юр лица
	/// </summary>
	public class LegalCounterpartyContacts
	{
		protected LegalCounterpartyContacts(IEnumerable<PhoneDto> phones, IEnumerable<EmailDto> emails)
		{
			Phones = phones;
			Emails = emails;
		}
		
		/// <summary>
		/// Список телефонов
		/// </summary>
		IEnumerable<PhoneDto> Phones { get; set; }
		/// <summary>
		/// Список электронных почт
		/// </summary>
		IEnumerable<EmailDto> Emails { get; set; }

		public static LegalCounterpartyContacts Create(IEnumerable<PhoneDto> phones, IEnumerable<EmailDto> emails) =>
			new LegalCounterpartyContacts(phones, emails);
	}
}
