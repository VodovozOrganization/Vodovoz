using System;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.ViewModels.TrueMark.CodesPool
{
	public class CodesPoolProblemDataNode
	{
		public EdoTask EdoTask { get; set; }
		public int EdoTaskId { get; set; }
		
		public int OrderId { get; set; }
		
		public string ErrorName { get; set; }
		
		public DateTime EdoTaskStartDate { get; set; }
	}
}
