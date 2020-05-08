using System;
using QS.ViewModels;
using QS.Commands;
using Vodovoz.Domain.Client;
using QS.DomainModel.Entity;
using System.ComponentModel;
using Vodovoz.Domain.Comments;
using Vodovoz.Repositories.HumanResources;

namespace Vodovoz.ViewModels.Comments
{
	public class DocumentCommentViewModel : WidgetViewModelBase
	{
		private readonly ICommentedDocument commentedDocument;

		/*
		private string lastComment;
		public string LastComment {
			get => lastComment;
			set => SetField(ref lastComment, value);
		}*/

		//public string tempStr { get; set; }

		public DocumentCommentViewModel(ICommentedDocument commentedDocument)
		{
			CreateCommands();
			//FillComment();
			this.commentedDocument = commentedDocument;
		}

		private string comment;
		public string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		/*
		private void FillComment()
		{

		}
		*/

		private void CreateCommands()
		{
			CreateAddCommentCommand();
			CreateRevertLastCommentCommand();
		}

		public DelegateCommand<string> AddCommentCommand { get; private set; }
		private void CreateAddCommentCommand()
		{
			AddCommentCommand = new DelegateCommand<string>(
				text => {
					//Незнаю, возможно тут какиенибудь проверки о возможности добавления комментария

					var sdsds = new DocumentComment();
					sdsds.Comment = text;
					sdsds.Author = CurrentEmployee;

					commentedDocument.AddComment(sdsds);
					/*Comment += LastComment + Environment.NewLine;
					tempStr = LastComment;
					LastComment = string.Empty;*/

				},

				//А может быть тут какиенибудь проверки о возможности добавления комментария
				text => !string.IsNullOrEmpty(text)
			); 
		}

		public DelegateCommand RevertLastCommentCommand { get; private set; }
		private void CreateRevertLastCommentCommand()
		{
			RevertLastCommentCommand = new DelegateCommand(

				() => {
					//Код получения последнего комментария из документа
					DocumentComment lastComment = null;

					//Код проверки возможности удаления комментария для текущего пользователя

					//Код удаления найденного комментария из документа
					commentedDocument.DeleteComment(lastComment);


					//Comment = Comment.Remove(Comment.Length - tempStr.Length - 1, tempStr.Length + 1);
					//LastComment = string.Empty;
				},
				() => true
			);
		}
	}
}
