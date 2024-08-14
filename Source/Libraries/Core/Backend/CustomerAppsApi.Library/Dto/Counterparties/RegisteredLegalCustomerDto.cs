﻿namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Информация о зарегистрированном юр лице в ERP
	/// </summary>
	public class RegisteredLegalCustomerDto
	{
		protected RegisteredLegalCustomerDto() { }
		
		private RegisteredLegalCustomerDto(
			int counterpartyId,
			string name,
			string inn,
			string kpp,
			string jurAddress,
			string shortTypeOfOwnership)
		{
			ErpCounterpartyId = counterpartyId;
			Name = name;
			Inn = inn;
			Kpp = kpp;
			JurAddress = jurAddress;
			ShortTypeOfOwnership = shortTypeOfOwnership;
		}
		
		/// <summary>
		/// Id созданного клиента в ДВ
		/// </summary>
		public int ErpCounterpartyId { get; }
		/// <summary>
		/// Наименование
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// ИНН
		/// </summary>
		public string Inn { get; }
		/// <summary>
		/// КПП
		/// </summary>
		public string Kpp { get; }
		/// <summary>
		/// Юр адрес
		/// </summary>
		public string JurAddress { get; }
		/// <summary>
		/// Сокращенное наименование формы собственности
		/// </summary>
		public string ShortTypeOfOwnership { get; }

		public static RegisteredLegalCustomerDto Create(
			int counterpartyId,
			string name,
			string inn,
			string kpp,
			string jurAddress,
			string shortTypeOfOwnership) =>
			new RegisteredLegalCustomerDto(counterpartyId, name, inn, kpp, jurAddress, shortTypeOfOwnership);
	}
}
