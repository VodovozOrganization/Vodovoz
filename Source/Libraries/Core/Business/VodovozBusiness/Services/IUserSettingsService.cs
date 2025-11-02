using Vodovoz.Core.Domain.Users.Settings;

namespace Vodovoz.Services
{
	public interface IUserSettingsService
	{
		UserSettings Settings { get; }
	}
}
