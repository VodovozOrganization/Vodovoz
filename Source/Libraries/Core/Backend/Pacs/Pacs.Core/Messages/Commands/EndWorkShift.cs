namespace Pacs.Core.Messages.Commands
{
	/// <summary>
	/// Команда завершения смены
	/// </summary>
	public class EndWorkShift : OperatorCommand
	{
		/// <summary>
		/// Причина завершения смены
		/// </summary>
		public string Reason { get; set; }
	}
}
