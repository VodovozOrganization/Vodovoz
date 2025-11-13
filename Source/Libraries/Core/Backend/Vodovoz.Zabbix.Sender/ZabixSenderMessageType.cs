namespace Vodovoz.Zabbix.Sender
{
	/// <summary>
	/// Тип сообщения Zabbix
	/// </summary>
	public enum ZabixSenderMessageType
	{
		/// <summary>
		/// Работает
		/// </summary>
		Up,

		/// <summary>
		/// Проблема
		/// </summary>
		Problem,

		/// <summary>
		/// Проблема решена
		/// </summary>
		SolvedProblem
	}
}
