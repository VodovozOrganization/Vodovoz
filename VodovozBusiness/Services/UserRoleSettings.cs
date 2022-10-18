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

		public int GetDefaultUserRoleId => _parametersProvider.GetIntValue("default_user_role_id");
		public int GetDefaultAvailableDatabaseId => _parametersProvider.GetIntValue("default_available_database_id");
	}
}
