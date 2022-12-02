using System;
using Vodovoz.Parameters;

namespace Vodovoz.Services
{
	public class UserRoleSettings : IUserRoleSettings
	{
		private readonly IParametersProvider _parametersProvider;

		public UserRoleSettings(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public string GetDefaultUserRoleName => _parametersProvider.GetStringValue("default_user_role_name");
		public string GetDatabaseForNewUser => _parametersProvider.GetStringValue("database_for_new_user");
	}
}
