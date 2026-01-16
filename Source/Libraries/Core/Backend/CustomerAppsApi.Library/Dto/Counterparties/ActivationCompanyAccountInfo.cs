namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Информация об активации аккаунта юр лица
	/// </summary>
	public class ActivationCompanyAccountInfo
	{
		/// <summary>
		/// Статус добавления номера телефона
		/// </summary>
		public string AddingPhoneNumberState { get; set; }
		/// <summary>
		/// Статус указания цели покупки воды
		/// </summary>
		public string AddingReasonForLeavingState { get; set; }
		/// <summary>
		/// Статус подключения ЭДО
		/// </summary>
		public string AddingEdoAccountState { get; set; }
		/// <summary>
		/// Статус проверки в ФНС
		/// </summary>
		public string TaxServiceCheckState { get; set; }
		/// <summary>
		/// Статус проверки в ЧЗ
		/// </summary>
		public string TrueMarkCheckState { get; set; }
		
		public static ActivationCompanyAccountInfo Create() =>
			new ActivationCompanyAccountInfo();
	}
}
