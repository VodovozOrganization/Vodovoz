
namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Информация для получения юр лиц по ИНН
	/// </summary>
	public class GetLegalCustomersByInnDto : GetLegalCustomersDto
	{
		/// <summary>
		/// ИНН
		/// </summary>
		public string Inn { get; set; }
		/// <summary>
		/// Электронная почта
		/// </summary>
		public string Email { get; set; }
	}
}
