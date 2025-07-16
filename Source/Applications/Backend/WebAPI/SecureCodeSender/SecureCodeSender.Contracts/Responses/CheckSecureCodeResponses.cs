namespace SecureCodeSender.Contracts.Responses
{
	public static class CheckSecureCodeResponses
	{
		public static (int Response, string Message) WrongCode => (404, "Неверный код доступа, пожалуйста, попробуйте еще раз");
		public static (int Response, string Message) CodeHasExpired => (408, "Время действия кода истекло, попробуйте еще раз");
		public static (int Response, string Message) Ok => (200, null);
	}
}
