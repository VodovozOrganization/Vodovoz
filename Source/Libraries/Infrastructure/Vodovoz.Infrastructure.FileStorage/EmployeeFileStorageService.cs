using QS.Project.DB;
using System;
using Vodovoz.Application.FileStorage;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class EmployeeFileStorageService : AttachedFilesAndPhotoFileStorageByS3Base<Employee, EmployeeFileInformation>, IEmployeeFileStorageService
	{
		private readonly IDatabaseConnectionSettings _databaseConnectionSettings;

		public EmployeeFileStorageService(IS3FileStorageService s3FileStorageService, IDatabaseConnectionSettings databaseConnectionSettings)
			: base(s3FileStorageService)
		{
			_databaseConnectionSettings = databaseConnectionSettings ?? throw new ArgumentNullException(nameof(databaseConnectionSettings));
		}

		protected override string BucketName => $"{_databaseConnectionSettings.DatabaseName.ToLower().Replace("_", "-")}-employee-attachments";
	}
}
