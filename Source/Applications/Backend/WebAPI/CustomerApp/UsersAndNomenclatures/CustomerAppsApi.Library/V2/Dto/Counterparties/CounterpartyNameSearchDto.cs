namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Запрос поиска контрагента по ФИО
	/// </summary>
	public class CounterpartyNameSearchDto
	{
		/// <summary>
		/// Полное ФИО одной строкой
		/// </summary>
		public string FullName { get; set; }
		/// <summary>
		/// Фамилия
		/// </summary>
		public string Surname { get; set; }
		/// <summary>
		/// Имя
		/// </summary>
		public string FirstName { get; set; }
		/// <summary>
		/// Отчество
		/// </summary>
		public string Patronymic { get; set; }
	}
}
