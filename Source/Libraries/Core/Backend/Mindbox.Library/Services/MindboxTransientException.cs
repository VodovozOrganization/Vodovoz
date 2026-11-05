using System;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Временная ошибка отправки в Mindbox, при которой имеет смысл повторить попытку
	/// (HTTP 500/502/503/504, таймаут, ошибка соединения, пауза Mindbox).
	/// </summary>
	public sealed class MindboxTransientException : Exception
	{
		public MindboxTransientException(string message)
			: base(message)
		{
		}

		public MindboxTransientException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
