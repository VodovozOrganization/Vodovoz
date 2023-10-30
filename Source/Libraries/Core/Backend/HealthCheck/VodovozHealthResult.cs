using System.Collections.Generic;

namespace VodovozHealthCheck
{
	public class VodovozHealthResult
	{
		public bool IsHealthy { get; set; }
		public List<string> AdditionalResults { get; set; } = new ();
	}
}
