using System.Collections.Generic;

namespace DriverAPI.Services
{
	public interface IWakeUpDriverClientService
	{
		IList<string> Clients { get; }
	}
}
