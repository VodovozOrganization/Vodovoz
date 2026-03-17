using System.Collections.Generic;

namespace Mango.Contracts.V1.Options
{
	public class MangoGroupOptions
	{
		public const string SectionName = "MangoGroup";
		
		public List<long> TargetGroupIds { get; set; }
	}
}
