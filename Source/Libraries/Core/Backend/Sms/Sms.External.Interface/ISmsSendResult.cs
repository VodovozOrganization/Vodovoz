namespace Sms.External.Interface
{
	/// <summary>
	/// Результат отправки сообщения
	/// </summary>
	public interface ISmsSendResult
	{
		/// <summary>
		/// Статус сообщения
		/// </summary>
		SmsSentStatus Status { get; }

		/// <summary>
		/// Уникальный идентификатор сообщения на стороне сервера
		/// </summary>
		string ServerId { get; }

		/// <summary>
		/// Идентификатор сообщения присвоенный на стороне клиента перед отправкой
		/// </summary>
		string LocalId { get; }

		/// <summary>
		/// Описание
		/// </summary>
		string Description { get; }
	}
}
