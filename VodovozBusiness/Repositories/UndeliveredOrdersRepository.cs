using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using NHibernate.Transform;
using QSOrmProject;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Repositories
{
	public static class UndeliveredOrdersRepository
	{
		public static Dictionary<GuiltyTypes, int> GetDictionaryWithUndeliveriesCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			UndeliveredOrder undeliveredOrderAlias = null;
			Order orderAlias = null;
			var query = uow.Session.QueryOver<UndeliveredOrder>(() => undeliveredOrderAlias);

			if(start != null && end != null)
				query.Left.JoinAlias(u => u.OldOrder, () => orderAlias)
					 .Where(() => orderAlias.DeliveryDate >= start)
					 .Where(u => orderAlias.DeliveryDate <= end);

			var result = query.SelectList(list => list
										  .SelectGroup(() => undeliveredOrderAlias.GuiltySide)
										  .SelectCount(() => undeliveredOrderAlias.Id)
										 )
							  .List<object[]>();

			return result.ToDictionary(x => (GuiltyTypes)x[0], x => (int)x[1]);
		}

		public static IList<UndeliveredOrderCountNode> GetListOfUndeliveriesCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			UndeliveredOrderCountNode resultAlias = null;
			UndeliveredOrder undeliveredOrderAlias = null;
			Order orderAlias = null;

			var query = uow.Session.QueryOver<UndeliveredOrder>(() => undeliveredOrderAlias)
			               .Left.JoinAlias(u => u.OldOrder, () => orderAlias);

			if(start != null && end != null)
				query.Where(() => orderAlias.DeliveryDate >= start)
					 .Where(u => orderAlias.DeliveryDate <= end);

			var result = query.SelectList(list => list
										  .SelectGroup(() => undeliveredOrderAlias.GuiltySide).WithAlias(() => resultAlias.Type)
										  .SelectCount(() => undeliveredOrderAlias.Id).WithAlias(() => resultAlias.Count)
										 )
			                  .TransformUsing(Transformers.AliasToBean<UndeliveredOrderCountNode>())
			                  .List<UndeliveredOrderCountNode>();

			return result;
		}

		public static IList<UndeliveredOrderCountNode> GetListOfUndeliveriesCountOnDptForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			UndeliveredOrderCountNode resultAlias = null;
			UndeliveredOrder undeliveredOrderAlias = null;
			Subdivision subdivisionAlias = null;
			Order orderAlias = null;

			var query = uow.Session.QueryOver<UndeliveredOrder>(() => undeliveredOrderAlias)
						   .Where(u => u.GuiltySide == GuiltyTypes.Department)
						   .Left.JoinAlias(u => u.OldOrder, () => orderAlias)
						   .Left.JoinAlias(u => u.GuiltyDepartment, () => subdivisionAlias);

			if(start != null && end != null)
				query.Where(() => orderAlias.DeliveryDate >= start)
					 .Where(u => orderAlias.DeliveryDate <= end);

			var result = query.SelectList(list => list
			                              .SelectGroup(() => subdivisionAlias.Id).WithAlias(() => resultAlias.SubdivisionId)
			                              .Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Subdivision)
										  .SelectCount(() => undeliveredOrderAlias.Id).WithAlias(() => resultAlias.Count)
										 )
			                  .TransformUsing(Transformers.AliasToBean<UndeliveredOrderCountNode>())
			                  .List<UndeliveredOrderCountNode>();

			return result;
		}

		public static IList<UndeliveredOrder> GetListOfUndeliveriesForOrder(IUnitOfWork uow, Order order){
			var query = uow.Session.QueryOver<UndeliveredOrder>()
			               .Where(u => u.OldOrder == order)
			               .List<UndeliveredOrder>();
			return query;
		}
	}

	public class UndeliveredOrderCountNode
	{
		public int SubdivisionId { get; set; }
		public GuiltyTypes Type { get; set; }
		public virtual int Count { get; set; }
		public string Subdivision { get; set; } = "Неизвестно";

		public virtual string GuiltySide => SubdivisionId <= 0 ? Type.GetEnumTitle() : Subdivision;
		public virtual string CountStr => Count.ToString();

		public virtual UndeliveredOrderCountNode Parent { get; set; } = null;
		public virtual List<UndeliveredOrderCountNode> Children { get; set; } = new List<UndeliveredOrderCountNode>();
	}
}
