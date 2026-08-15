using System;
using Vodovoz.Core.Domain.Users.Settings;

namespace VodovozBusiness.Services.Users
{
	public interface IUserSettingsManager : IDisposable
	{
		UserSettings Settings { get; }
		void SaveSettings();
	}
}
