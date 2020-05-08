using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Domain.Comments
{
	public interface ICommentedDocument
	{
		//Тут может быть лучше вынести Observable чтобы само все уведомлялось, незнаю
		event EventHandler CommentsChanged;
		IEnumerable<DocumentComment> Comments { get; }

		void AddComment(DocumentComment comment);

		//А может быть лучше метод: void DeleteLastComment();
		void DeleteComment(DocumentComment comment);
	}
}
