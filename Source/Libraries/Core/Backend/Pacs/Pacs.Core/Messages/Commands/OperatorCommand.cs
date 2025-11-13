namespace Pacs.Core.Messages.Commands
{
	/// <summary>
	/// Команда оператора
	/// </summary>
	public class OperatorCommand : CommandBase
	{
		/// <summary>
		/// Идентификатор оператора
		/// </summary>
		public int OperatorId { get; set; }
	}
}
