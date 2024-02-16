namespace Vodovoz.Settings.User
{
	public interface IUserRoleSettings
	{
		string GetDefaultUserRoleName { get; }
		string GetDatabaseForNewUser { get; }
	}
}
