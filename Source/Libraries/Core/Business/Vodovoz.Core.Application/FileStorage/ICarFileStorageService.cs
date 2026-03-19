using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Core.Application.FileStorage
{
	public interface ICarFileStorageService : IEntityFileStorageService<Car>, IEntityPhotoStorageService<Car>
	{
	}
}
