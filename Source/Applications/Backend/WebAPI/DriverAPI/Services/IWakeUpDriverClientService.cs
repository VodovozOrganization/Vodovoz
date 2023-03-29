using System.Collections.Generic;
using Vodovoz.Domain.Employees;

namespace DriverAPI.Services
{
	public interface IWakeUpDriverClientService
	{
		IReadOnlyDictionary<int, string> Clients { get; }

		void Subscribe(Employee driver, string token);

		void UnSubscribe(Employee driver);
	}
}
