using Pacs.Server;

namespace Pacs.Core.Messages.Commands
{
	public class AdminStartBreak : OperatorCommand
	{
		public int AdminId{ get; set; }
		public OperatorBreakType BreakType { get; set; }
		public string Reason { get; set; }
	}
}
