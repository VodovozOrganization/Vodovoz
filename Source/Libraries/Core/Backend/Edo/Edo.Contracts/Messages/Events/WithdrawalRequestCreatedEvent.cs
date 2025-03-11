namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Запрос на вывод из оборота создан
	/// </summary>
	public class WithdrawalRequestCreatedEvent
	{
		/// <summary>
		/// Идентификатор запроса на вывод из оборота
		/// </summary>
		public int Id { get; set; }
	}
}
