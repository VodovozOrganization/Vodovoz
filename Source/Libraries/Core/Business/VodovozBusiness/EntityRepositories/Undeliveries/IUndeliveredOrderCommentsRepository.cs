using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Undeliveries
{
	public interface IUndeliveredOrderCommentsRepository
	{
		IList<UndeliveredOrderComment> GetComments(IUnitOfWork uow, UndeliveredOrder undeliveredOrder, CommentedFields field);
		IList<UndeliveredOrderCommentsNode> GetCommentNodes(IUnitOfWork uow, UndeliveredOrder undeliveredOrder, CommentedFields field);
	}
}