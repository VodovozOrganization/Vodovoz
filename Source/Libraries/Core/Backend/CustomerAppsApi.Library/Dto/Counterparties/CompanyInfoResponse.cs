namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Ответ эндпойнта получения информации о компании
	/// </summary>
	public class CompanyInfoResponse
	{
		/// <summary>
		/// Наименование юр лица
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// ИНН
		/// </summary>
		public string Inn { get; set; }
		/// <summary>
		/// КПП
		/// </summary>
		public string Kpp { get; set; }
		/// <summary>
		/// Юр адрес
		/// </summary>
		public string Address { get; set; }
		/// <summary>
		/// Расчетный счет
		/// </summary>
		public string AccountNumber { get; set; }
		/// <summary>
		/// Отсрочка платежа
		/// </summary>
		public int DelayOfPayment { get; set; }
		/// <summary>
		/// Статус подключения ЭДО
		/// </summary>
		public string AddingEdoAccountState { get; set; }
		/// <summary>
		/// Статус проверки в ФНС
		/// </summary>
		public string TaxServiceCheckState { get; set; }
	}
}
