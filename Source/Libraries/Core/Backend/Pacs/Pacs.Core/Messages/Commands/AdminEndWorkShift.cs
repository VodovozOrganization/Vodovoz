namespace Pacs.Core.Messages.Commands
{
	public class AdminEndWorkShift : OperatorCommand
	{
		public int AdminId { get; set; }
		public string Reason { get; set; }
	}
}
