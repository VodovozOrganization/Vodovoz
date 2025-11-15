using System;

namespace Mailganer.Api.Client.Exceptions
{
	/// <summary>
	/// Email адрес находится в стоп-листе
	/// </summary>
	public class EmailInStopListException : Exception
	{
		/// <summary>
		/// Конструктор исключения для email адреса, находящегося в стоп-листе
		/// </summary>
		/// <param name="email">Email</param>
		/// <param name="bounceMessage">Причина добавления в стоп-лист</param>
		public EmailInStopListException(string email, string bounceMessage)
		{
			Email = email;
			BounceMessage = bounceMessage;
		}

		/// <summary>
		/// Email адрес
		/// </summary>
		public string Email { get; }

		/// <summary>
		/// Причина добавления в стоп-лист
		/// </summary>
		public string BounceMessage { get; }

		/// <summary>
		/// Сообщение исключения
		/// </summary>
		public override string Message => $"Email '{Email}' is in stop list. Bounce message: {BounceMessage}";
	}
}
