using System;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.Repositories
{
	public class EdoProblemNode
	{
		public int OrderTaskId { get; set; }
		public int TransferTaskId { get; set; }
		public DateTime Time { get; set; }
		public TaskProblemState State { get; set; }
		public string Message { get; set; }
		public string Description { get; set; }
		public string Recommendation { get; set; }
	}
}
