namespace Vodovoz.Services
{
	public interface IUserRoleSettings
	{
		int GetDefaultUserRoleId { get; }
		int GetDefaultAvailableDatabaseId { get; }
	}
}
