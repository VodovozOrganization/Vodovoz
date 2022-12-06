namespace Vodovoz.Services
{
	public interface IUserRoleSettings
	{
		string GetDefaultUserRoleName { get; }
		string GetDatabaseForNewUser { get; }
	}
}
