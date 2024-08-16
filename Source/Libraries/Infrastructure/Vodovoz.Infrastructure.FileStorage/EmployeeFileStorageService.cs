using System;
using Vodovoz.Application.FileStorage;
using Vodovoz.Domain.Employees;
using VodovozBusiness.Domain.Employees;

namespace Vodovoz.Infrastructure.FileStorage
{
	internal sealed class EmployeeFileStorageService : AttachedFilesAndPhotoFileStorageByS3Base<Employee, EmployeeFileInformation>, IEmployeeFileStorageService
	{
		public EmployeeFileStorageService(IS3FileStorageService s3FileStorageService)
			: base(s3FileStorageService)
		{
		}

		protected override string BucketName => throw new NotImplementedException();
	}
}
