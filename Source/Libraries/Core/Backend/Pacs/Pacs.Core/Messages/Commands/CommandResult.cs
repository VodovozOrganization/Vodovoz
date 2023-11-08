namespace Pacs.Core.Messages.Commands
{
	public abstract class CommandResult
	{
		public virtual Result Result { get; set; }
		public virtual string FailureDescription { get; set; }
	}

	public enum Result
	{
		Success,
		Failure
	}
}
