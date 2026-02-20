namespace FastPaymentsApi.Contracts.Responses
{
	/// <summary>
	/// Детали ответа от банка
	/// </summary>
	public interface IAvangardResponseDetails
	{
		/// <summary>
		/// Код ответа
		/// </summary>
		int ResponseCode { get; set; }
		/// <summary>
		/// Описание
		/// </summary>
		string ResponseMessage { get; set; }
	}
}
