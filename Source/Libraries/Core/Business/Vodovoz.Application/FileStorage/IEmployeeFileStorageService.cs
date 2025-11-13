using Vodovoz.Domain.Employees;

namespace Vodovoz.Application.FileStorage
{
	public interface IEmployeeFileStorageService : IEntityFileStorageService<Employee>, IEntityPhotoStorageService<Employee>
	{
	}
}
