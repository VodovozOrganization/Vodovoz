namespace FastPaymentsApi.Contracts
{
	/// <summary>
	/// Информация об успешной оплате
	/// </summary>
	public class PaidOrderDTO
	{
		/// <summary>
		/// Инфа об исполненном платеже в виде xml, в строковом формате
		/// </summary>
		public string xml { get; set; }
	}
}
