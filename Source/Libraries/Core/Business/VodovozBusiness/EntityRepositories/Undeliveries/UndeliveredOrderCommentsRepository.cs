using System.Collections.Generic;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Undeliveries
{
	public class UndeliveredOrderCommentsRepository : IUndeliveredOrderCommentsRepository
	{
		public IList<UndeliveredOrderComment> GetComments(IUnitOfWork uow, UndeliveredOrder undeliveredOrder, CommentedFields field)
		{
			return uow.Session.QueryOver<UndeliveredOrderComment>()
					  .Where(c => c.UndeliveredOrder.Id == undeliveredOrder.Id)
					  .Where(c => c.CommentedField == field)
					  .List<UndeliveredOrderComment>();
		}

		public IList<UndeliveredOrderCommentsNode> GetCommentNodes(IUnitOfWork uow, UndeliveredOrder undeliveredOrder, CommentedFields field)
		{
			UndeliveredOrderCommentsNode resultAlias = null;
			UndeliveredOrderComment undeliveredOrderAllais = null;
			Employee employeeAlias = null;
			bool clr = false;

			var nodes = uow.Session.QueryOver<UndeliveredOrderComment>(() => undeliveredOrderAllais)
			               .Where(c => c.UndeliveredOrder.Id == undeliveredOrder.Id)
			               .Where(c => c.CommentedField == field)
			               .JoinAlias(() => undeliveredOrderAllais.Employee, () => employeeAlias)
			               .SelectList(
				               list => list
				               .Select(() => undeliveredOrderAllais.Comment).WithAlias(() => resultAlias.Comment)
				               .Select(() => undeliveredOrderAllais.CommentDate).WithAlias(() => resultAlias.Date)
				               .Select(() => employeeAlias.Name).WithAlias(() => resultAlias.FName)
				               .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.MName)
				               .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.LName)
				              )
			               .OrderBy(i=>i.CommentDate).Desc
			               .TransformUsing(Transformers.AliasToBean<UndeliveredOrderCommentsNode>())
			               .List<UndeliveredOrderCommentsNode>();
			
			return nodes;
		}
	}
}
