using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Core.Messages.Commands
{
	/// <summary>
	/// Команда начала перерыва
	/// </summary>
	public class StartBreak : OperatorCommand
	{
		/// <summary>
		/// Тип перерыва
		/// </summary>
		public OperatorBreakType BreakType { get; set; }
	}
}
