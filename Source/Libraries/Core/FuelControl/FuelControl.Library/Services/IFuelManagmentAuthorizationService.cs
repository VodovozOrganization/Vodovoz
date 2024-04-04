using FuelControl.Contracts.Requests;
using System.Threading.Tasks;

namespace FuelControl.Library.Services
{
	public interface IFuelManagmentAuthorizationService
	{
		Task<string> Login(AuthorizationRequest authorizationRequest);
	}
}
