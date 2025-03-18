using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server.Operators
{
	public class WorkShiftEndArgs
	{
		public WorkShiftChangedBy WorkShiftChangedBy { get; set; }
		public int? AdminId { get; set; }
		public string Reason { get; set; }
	}
}
