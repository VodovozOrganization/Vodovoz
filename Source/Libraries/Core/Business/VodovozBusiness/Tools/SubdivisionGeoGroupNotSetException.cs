using System;
using System.Runtime.Serialization;

namespace Vodovoz.Tools
{
	/// <summary>
	/// Не указана обслуживаемая часть города у подразделения сотрудника
	/// </summary>
	public class SubdivisionGeoGroupNotSetException : InvalidOperationException
	{
		/// <summary>
		/// Создание исключения без сообщения
		/// </summary>
		public SubdivisionGeoGroupNotSetException()
		{
		}

		/// <summary>
		/// Создание исключения с сообщением
		/// </summary>
		/// <param name="message">Сообщение об ошибке</param>
		public SubdivisionGeoGroupNotSetException(string message) : base(message)
		{
		}

		/// <summary>
		/// Создание исключения с сообщением и вложенным исключением
		/// </summary>
		/// <param name="message">Сообщение об ошибке</param>
		/// <param name="innerException">Исключение, вызвавшее текущее</param>
		public SubdivisionGeoGroupNotSetException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		/// Создание исключения при десериализации
		/// </summary>
		/// <param name="info">Данные сериализации</param>
		/// <param name="context">Контекст потока сериализации</param>
		protected SubdivisionGeoGroupNotSetException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
