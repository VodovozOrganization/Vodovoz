using System.Collections.Generic;

namespace VodovozHealthCheck.Dto
{
	public class VodovozHealthResultDto
	{
		public bool IsHealthy { get; set; }
		public List<string> AdditionalUnhealthyResults { get; set; } = new ();
	}
}
