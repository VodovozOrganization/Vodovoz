using System.Threading;
using System.Threading.Tasks;

namespace FuelControl.Library.Services
{
	public interface IFuelControlAuthorizationService
	{
		Task<string> Login(string login, string password, string apiKey, CancellationToken cancellationToken);
	}
}
