using System.Collections.Generic;

namespace VodovozHealthCheck
{
	public class VodovozHealthResultDto
	{
		public bool IsHealthy { get; set; }
		public List<string> AdditionalResults { get; set; } = new ();
	}
}
