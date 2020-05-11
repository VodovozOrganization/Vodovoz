using System;
using QS.ViewModels;
using QS.Commands;
using Vodovoz.Domain.Comments;
using System.Linq;
using Vodovoz.EntityRepositories.Employees;
using QS.DomainModel.UoW;

namespace Vodovoz.ViewModels.Comments
{
	public class DocumentCommentViewModel : WidgetViewModelBase
	{
		private string comment;
		public string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		private string curComment;
		public string CurComment {
			get => curComment;
			set => SetField(ref curComment, value);
		}

		private bool CommentAdded { get; set; }

		readonly ICommentedDocument commentedDocument;
		readonly IEmployeeRepository employeeRepository;
		readonly IUnitOfWork uow;

		public DocumentCommentViewModel(ICommentedDocument commentedDocument, IEmployeeRepository employeeRepository, IUnitOfWork uow)
		{
			this.commentedDocument = commentedDocument;
			this.uow = uow;
			this.employeeRepository = employeeRepository;

			CreateCommands();

			if(commentedDocument.Comments.Any())
				FillComment();
		}

		private void FillComment()
		{
			foreach(DocumentComment item in commentedDocument.Comments) {
				Comment += item.Comment;
			}
		}

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

					var employee = employeeRepository.GetEmployeeForCurrentUser(uow);

					var str = $"{employee.ShortName}({employee?.Subdivision?.ShortName ?? employee?.Subdivision?.Name}) " +
						$"{DateTime.Now.ToString("dd/MM/yyyy HH:mm")}: ";

					var newComment = new DocumentComment {
						Comment = str + text + Environment.NewLine,
						Author = employee
					};

					commentedDocument.AddComment(newComment);
					Comment += newComment.Comment;

					CommentAdded = true;
					CurComment = string.Empty;
				},

				text => !string.IsNullOrEmpty(text)
			); 
		}

		public DelegateCommand RevertLastCommentCommand { get; private set; }
		private void CreateRevertLastCommentCommand()
		{
			RevertLastCommentCommand = new DelegateCommand(

				() => {
					//Код получения последнего комментария из документа
					DocumentComment lastComment = commentedDocument.Comments.LastOrDefault();

					//Код проверки возможности удаления комментария для текущего пользователя

					if(lastComment != null) {

						commentedDocument.DeleteLastComment(lastComment);
						Comment = Comment.Remove(Comment.Length - lastComment.Comment.Length, lastComment.Comment.Length);
					}

					CommentAdded = false;
				},
				() => CommentAdded == true
			);
		}
	}
}
