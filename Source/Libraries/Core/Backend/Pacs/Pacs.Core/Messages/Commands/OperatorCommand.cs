namespace Pacs.Core.Messages.Commands
{
	/// <summary>
	/// Команда оператора
	/// </summary>
	public abstract class OperatorCommand
	{
		/// <summary>
		/// Идентификатор оператора
		/// </summary>
		public int OperatorId { get; set; }
	}
}
