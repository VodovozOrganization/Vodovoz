using System.Threading.Tasks;

namespace FuelControl.Library.Services
{
	public interface IFuelManagmentAuthorizationService
	{
		Task<string> Login(string login, string password, string apiKey);
	}
}
