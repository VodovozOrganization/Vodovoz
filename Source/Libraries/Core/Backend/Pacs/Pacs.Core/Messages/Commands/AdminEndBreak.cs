namespace Pacs.Core.Messages.Commands
{
	public class AdminEndBreak : OperatorCommand
	{
		public int AdminId{ get; set; }
		public string Reason { get; set; }
	}
}
