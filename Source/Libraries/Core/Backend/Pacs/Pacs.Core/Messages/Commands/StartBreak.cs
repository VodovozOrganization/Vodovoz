using Pacs.Server;

namespace Pacs.Core.Messages.Commands
{
	public class StartBreak : OperatorCommand
	{
		public OperatorBreakType BreakType { get; set; }
	}
}
