using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Interfaces.Logistics.Cars;
using Vodovoz.Core.Domain.Schemas.Logistics;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Logistics.Cars
{
	public class CarIdRepository : ICarIdRepository
	{
		public int? GetCarIdByEmployeeId(IUnitOfWork uow, int employeeId)
		{
			var query =
				$"Select id from {CarSchema.TableName}" +
				$" where {CarSchema.TableName}.{CarSchema.DriverIdColumn} = {employeeId}";
			
			var result = (int)uow.Session.CreateSQLQuery(query)
				.UniqueResult<uint>();

			if(result == default)
			{
				return null;
			}

			return result;
		}
	}
}
