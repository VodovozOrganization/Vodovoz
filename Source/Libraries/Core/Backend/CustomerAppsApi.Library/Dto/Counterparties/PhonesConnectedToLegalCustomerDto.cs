using System.Collections.Generic;
using Vodovoz.Core.Data.Counterparties;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Список телефонов, которые доступны для заказа от имени юр лица, с состоянием связи
	/// </summary>
	public class PhonesConnectedToLegalCustomerDto
	{
		private PhonesConnectedToLegalCustomerDto(IEnumerable<PhoneInfo> phones)
		{
			Phones = phones;
		}
		
		/// <summary>
		/// Список телефонов
		/// </summary>
		public IEnumerable<PhoneInfo> Phones { get; }

		public static PhonesConnectedToLegalCustomerDto Create(IEnumerable<PhoneInfo> phones) =>
			new PhonesConnectedToLegalCustomerDto(phones);
	}
}
