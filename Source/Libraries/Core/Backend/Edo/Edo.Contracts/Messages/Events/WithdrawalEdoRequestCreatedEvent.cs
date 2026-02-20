namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие о создании заявки на вывод кодов из оборота
	/// </summary>
	public class WithdrawalEdoRequestCreatedEvent
	{
		/// <summary>
		/// Идентификатор созданной заявки на вывод из оборота
		/// </summary>
		public int Id { get; set; }
	}
}
