using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Application.FileStorage
{
	public interface ICarFileStorageService : IEntityFileStorageService<Car>, IEntityPhotoStorageService<Car>
	{
	}
}
