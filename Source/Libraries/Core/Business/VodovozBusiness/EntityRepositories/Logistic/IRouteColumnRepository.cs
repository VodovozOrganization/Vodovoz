using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface IRouteColumnRepository
	{
		IList<RouteColumn> ActiveColumns(IUnitOfWork uow);
		IList<Nomenclature> NomenclaturesForColumn(IUnitOfWork uow, RouteColumn column);
	}
}