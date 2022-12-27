using Vodovoz.Additions;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Infrastructure.Services;

namespace Vodovoz.Factories
{
	public class AuthorizationServiceFactory : IAuthorizationServiceFactory
	{
		private static readonly IParametersProvider _parametersProvider = new ParametersProvider(); 
		
		public IAuthorizationService CreateNewAuthorizationService() =>
			new AuthorizationService(
				new PasswordGenerator(),
				new UserRoleSettings(_parametersProvider),
				new UserRoleRepository(),
				new EntityRepositories.UserRepository(),
				new EmailParametersProvider(_parametersProvider),
				new SubdivisionParametersProvider(_parametersProvider));
	}
}
