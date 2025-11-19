namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Информация об активации юр лица
	/// </summary>
	public class ActivationLinkCompany
	{
		/// <summary>
		/// Статус добавления номера телефона
		/// </summary>
		public string PhoneNumber { get; set; }
		/// <summary>
		/// Статус указания цели покупки воды
		/// </summary>
		public string PurposeOfPurchase { get; set; }
		/// <summary>
		/// Статус подключения ЭДО
		/// </summary>
		public string EDocumentSystem { get; set; }
		/// <summary>
		/// Статус проверки в ФНС
		/// </summary>
		public string TaxServiceCheck { get; set; }
		/// <summary>
		/// Статус проверки в ЧЗ
		/// </summary>
		public string MarkingSystemCheck { get; set; }
	}
}
