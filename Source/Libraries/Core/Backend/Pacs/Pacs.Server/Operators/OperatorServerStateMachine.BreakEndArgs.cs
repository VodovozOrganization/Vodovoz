using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Operators
{
	public partial class OperatorServerStateMachine
	{
		private class BreakEndArgs
		{
			public BreakChangedBy BreakChangedBy { get; set; }
			public int AdminId { get; set; }
			public string Reason { get; set; }
		}
	}
}
