using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Operators
{
	public partial class OperatorServerStateMachine
	{
		private class BreakStartArgs
		{
			public BreakChangedBy BreakChangedBy { get; set; }
			public OperatorBreakType BreakType { get; set; }
			public int AdminId { get; set; }
			public string Reason { get; set; }
		}
	}
}
