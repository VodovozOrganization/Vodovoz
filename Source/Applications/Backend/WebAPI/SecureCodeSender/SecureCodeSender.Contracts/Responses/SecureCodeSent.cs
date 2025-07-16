namespace SecureCodeSender.Contracts.Responses
{
	/// <summary>
	/// Ответ по отправленному коду авторизации
	/// </summary>
	public class SecureCodeSent
	{
		private SecureCodeSent(int timeForNextCode)
		{
			TimeForNextCode = timeForNextCode;
		}
		
		/// <summary>
		/// Время до запроса следующего кода(секунды)
		/// </summary>
		public int TimeForNextCode { get; }
		
		/// <summary>
		/// Метод создания нового экземпляра
		/// </summary>
		/// <param name="timeForNextCode">Ыремя до следующего запроса кода</param>
		/// <returns></returns>
		public static SecureCodeSent Create(int timeForNextCode) => new SecureCodeSent(timeForNextCode);
	}
}
