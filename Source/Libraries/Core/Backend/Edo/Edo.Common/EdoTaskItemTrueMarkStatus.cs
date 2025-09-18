using TrueMark.Contracts;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Common
{
	public class EdoTaskItemTrueMarkStatus
	{
		public EdoTaskItem EdoTaskItem { get; set; }
		public ProductInstanceStatus ProductInstanceStatus { get; set; }
		public EdoTaskItemCodeType ItemCodeType { get; set; }
	}
}
