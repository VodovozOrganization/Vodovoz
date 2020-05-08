using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Comments;
namespace Vodovoz.Domain.BusinessTasks
{
	public class PaymentTask : BusinessTask, ICommentedDocument
	{
		public PaymentTask()
		{
		}

		public GenericObservableList<DocumentComment> Comments => throw new System.NotImplementedException();

		public void AddComment(DocumentComment comment)
		{
			throw new System.NotImplementedException();
		}

		public void DeleteLastComment(DocumentComment comment)
		{
			throw new System.NotImplementedException();
		}
	}
}
