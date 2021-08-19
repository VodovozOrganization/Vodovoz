using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class RouteColumnRepository : IRouteColumnRepository
	{
		public IList<RouteColumn> ActiveColumns(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<RouteColumn>().List<RouteColumn>();
		}

		public IList<Nomenclature> NomenclaturesForColumn(IUnitOfWork uow, RouteColumn column)
		{
			return uow.Session.QueryOver<Nomenclature>()
				.Where(x => x.RouteListColumn.Id == column.Id)
				.List();
		}
	}
}

