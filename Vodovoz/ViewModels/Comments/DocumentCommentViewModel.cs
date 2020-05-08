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
		private string comment;
		public string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		private string lastComment;
		public string LastComment {
			get => lastComment;
			set => SetField(ref lastComment, value);
		}

		public string tempStr { get; set; }

		public DocumentCommentViewModel()
		{
			CreateCommands();
			FillComment();
		}

		private void FillComment()
		{

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

					Comment += LastComment + Environment.NewLine;
					tempStr = LastComment;
					LastComment = string.Empty;

				},
				text => !string.IsNullOrEmpty(text)
			); 
		}

		public DelegateCommand RevertLastCommentCommand { get; private set; }
		private void CreateRevertLastCommentCommand()
		{
			RevertLastCommentCommand = new DelegateCommand(

				() => {

					Comment = Comment.Remove(Comment.Length - tempStr.Length - 1, tempStr.Length + 1);
					
					LastComment = string.Empty;
				},
				() => true
			);
		}
	}
}
