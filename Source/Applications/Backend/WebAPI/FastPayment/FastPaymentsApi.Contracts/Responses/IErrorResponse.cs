namespace FastPaymentsApi.Contracts.Responses
{
	/// <summary>
	/// Сообщение с ошибкой
	/// </summary>
	public interface IErrorResponse
	{
		/// <summary>
		/// Сообщение об ошибке
		/// </summary>
		string ErrorMessage { get; set; }
	}
}
