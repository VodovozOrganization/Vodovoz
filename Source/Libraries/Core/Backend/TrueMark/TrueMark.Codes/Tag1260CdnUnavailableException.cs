using System;

namespace TrueMark.Library
{
	/// <summary>
	/// Исключение, возникающее при недоступности всех CDN хостов Честного Знака
	/// </summary>
	public class Tag1260CdnUnavailableException : Exception
	{
		/// <summary>
		/// Конструктор исключения недоступности CDN
		/// </summary>
		/// <param name="message">Сообщение об ошибке</param>
		public Tag1260CdnUnavailableException(string message) : base(message)
		{
		}

		/// <summary>
		/// Конструктор исключения с внутренним исключением
		/// </summary>
		/// <param name="message">Сообщение об ошибке</param>
		/// <param name="innerException">Внутреннее исключение</param>
		public Tag1260CdnUnavailableException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
