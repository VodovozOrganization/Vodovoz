using Vodovoz.ViewModels.Infrastructure.Services;

namespace Vodovoz.Factories
{
	public interface IAuthorizationServiceFactory
	{
		IAuthorizationService CreateNewAuthorizationService();
	}
}