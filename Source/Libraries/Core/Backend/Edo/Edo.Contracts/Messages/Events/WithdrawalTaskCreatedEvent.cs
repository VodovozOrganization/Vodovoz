namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Задача на вывод из оборота создана
	/// </summary>
	public class WithdrawalTaskCreatedEvent
	{
		/// <summary>
		/// Идентификатор задачи на вывод из оборота
		/// </summary>
		public int WithdrawalEdoTaskId { get; set; }
	}
}
