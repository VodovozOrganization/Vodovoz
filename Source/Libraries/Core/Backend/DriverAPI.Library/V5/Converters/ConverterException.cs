using System;

namespace DriverAPI.Library.V5.Converters
{
	/// <summary>
	/// Exception конвертации
	/// </summary>
	public class ConverterException : ArgumentOutOfRangeException
	{
		/// <summary>
		/// Констурктор
		/// </summary>
		/// <param name="paramName">Имяпараметра</param>
		/// <param name="actualValue">Значение</param>
		/// <param name="message">Сообщение</param>
		public ConverterException(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
		{
		}
	}
}
