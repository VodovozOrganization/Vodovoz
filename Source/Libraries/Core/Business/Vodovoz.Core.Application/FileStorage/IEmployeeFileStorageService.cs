using Vodovoz.Domain.Employees;

namespace Vodovoz.Core.Application.FileStorage
{
	public interface IEmployeeFileStorageService : IEntityFileStorageService<Employee>, IEntityPhotoStorageService<Employee>
	{
	}
}
