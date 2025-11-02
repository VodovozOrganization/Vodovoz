namespace Pacs.Core.Messages.Commands
{
	/// <summary>
	/// Завершение перерыва администратором
	/// </summary>
	public class AdminEndBreak : OperatorCommand
	{
		/// <summary>
		/// Идентификатор администратора
		/// </summary>
		public int AdminId { get; set; }

		/// <summary>
		/// Причина завершения перерыва
		/// </summary>
		public string Reason { get; set; }
	}
}
