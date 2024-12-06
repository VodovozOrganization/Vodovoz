namespace Pacs.Core.Messages.Commands
{
	/// <summary>
	/// Комманда завершения смены администратором
	/// </summary>
	public class AdminEndWorkShift : OperatorCommand
	{
		/// <summary>
		/// Идентификатор админитсратора
		/// </summary>
		public int AdminId { get; set; }

		/// <summary>
		/// Причина завершения смены
		/// </summary>
		public string Reason { get; set; }
	}
}
