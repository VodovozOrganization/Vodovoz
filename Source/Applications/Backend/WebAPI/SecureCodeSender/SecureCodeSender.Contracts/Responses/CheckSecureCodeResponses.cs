namespace SecureCodeSender.Contracts.Responses
{
	/// <summary>
	/// Сборник ответов по запросу кода авторизации
	/// </summary>
	public static class CheckSecureCodeResponses
	{
		/// <summary>
		/// Неверный код
		/// </summary>
		public static (int Response, string Message) WrongCode => (404, "Неверный код доступа, пожалуйста, попробуйте еще раз");
		/// <summary>
		/// Истекший код
		/// </summary>
		public static (int Response, string Message) CodeHasExpired => (408, "Время действия кода истекло, попробуйте еще раз");
		/// <summary>
		/// ОК
		/// </summary>
		public static (int Response, string Message) Ok => (200, null);
	}
}
