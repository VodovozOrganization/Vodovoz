using System.Collections.Generic;

namespace VodovozHealthCheck.Dto
{
	public class VodovozHealthResultDto
	{
		public bool IsHealthy { get; set; }
		public HashSet<string> AdditionalUnhealthyResults { get; set; } = new ();
	}
}
