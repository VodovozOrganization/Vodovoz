using System;
using QS.Project.DB;

namespace ResourceLocker.Library.Providers
{
	public class VodovozDesktopResourceLockerUniqueKeyProvider : IResourceLockerUniqueKeyProvider
	{
		private readonly IDatabaseConnectionSettings _dbSettings;

		public VodovozDesktopResourceLockerUniqueKeyProvider(IDatabaseConnectionSettings dbSettings)
		{
			_dbSettings = dbSettings ?? throw new ArgumentNullException(nameof(dbSettings));
		}

		public string GetResourceLockerUniqueKeyByResourceName(string resourceName)
		{
			return $"lock:{_dbSettings.DatabaseName}:{resourceName}";
		}
	}
}
