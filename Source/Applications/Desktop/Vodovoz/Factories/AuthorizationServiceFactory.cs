using Autofac;
using Vodovoz.ViewModels.Infrastructure.Services;

namespace Vodovoz.Factories
{
	public class AuthorizationServiceFactory : IAuthorizationServiceFactory
	{
		public IAuthorizationService CreateNewAuthorizationService() => MainClass.AppDIContainer.Resolve<IAuthorizationService>();
	}
}
