using System;
using QS.Project.DB;

namespace ResourceLocker.Library.Providers
{
	public class VodovozDesktopResourceLockerValueProvider : IResourceLockerValueProvider
	{
		private readonly IDatabaseConnectionSettings _dbSettings;
		private readonly Guid _guid;

		public VodovozDesktopResourceLockerValueProvider(IDatabaseConnectionSettings dbSettings)
		{
			_dbSettings = dbSettings ?? throw new ArgumentNullException(nameof(dbSettings));
			_guid = Guid.NewGuid();
		}

		public string GetResourceLockerValue()
		{
			return $"{_dbSettings.UserName}:{_guid}";
		}
	}
}
