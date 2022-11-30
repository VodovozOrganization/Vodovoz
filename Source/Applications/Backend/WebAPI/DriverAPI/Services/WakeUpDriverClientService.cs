using System.Collections.Generic;

namespace DriverAPI.Services
{
	public class WakeUpDriverClientService : IWakeUpDriverClientService
	{
		protected readonly List<string> _clients = new();

		public IList<string> Clients { get => _clients; }
	}
}
