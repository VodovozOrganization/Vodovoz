namespace Vodovoz.Core.Domain.Pacs
{
	public enum CallState
	{
		/// <summary>
		/// Звонок в режиме дозвона
		/// </summary>
		Appeared,

		/// <summary>
		/// Соединен с оператором
		/// </summary>
		Connected,

		/// <summary>
		/// Звонок на удержании
		/// </summary>
		OnHold,

		/// <summary>
		/// Звонок завершен
		/// </summary>
		Disconnected
	}
}
