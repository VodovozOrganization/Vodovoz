using Gtk;
using QS.Dialog;
using System;
using Vodovoz.Dialogs;

namespace Vodovoz.Services
{
	public class CastomInteractiveService : IInteractiveService
	{
		private readonly CastomInteractiveMessage _interactiveMessage = new CastomInteractiveMessage();
		private readonly CastomQuestion _question = new CastomQuestion();

		public bool Question(string message, string title = null)
		{
			return _question.Question(message, title);
		}

		public string Question(string[] buttons, string message, string title = null)
		{
			return _question.Question(buttons, message, title);
		}

		public void ShowMessage(ImportanceLevel level, string message, string title = null)
		{
			_interactiveMessage.ShowMessage(level, message, title);
		}
	}

	public class CastomInteractiveMessage : IInteractiveMessage
	{
		public void ShowMessage(ImportanceLevel level, string message, string title = null)
		{
			switch(level)
			{
				case ImportanceLevel.Error:
					CastomMessageDialogHelper.RunErrorDialog(message, title);
					break;
				case ImportanceLevel.Warning:
					CastomMessageDialogHelper.RunWarningDialog(message, title);
					break;
				case ImportanceLevel.Info:
				default:
					CastomMessageDialogHelper.RunInfoDialog(message, title);
					break;
			}
		}
	}

	public class CastomQuestion : IInteractiveQuestion
	{
		public bool Question(string message, string title = null)
		{
			CastomMessageDlg md = new CastomMessageDlg(null,
								   DialogFlags.Modal,
								   MessageType.Question,
								   ButtonsType.YesNo,
								   message);
			md.SetPosition(WindowPosition.Center);
			md.Title = title ?? "Вопрос";
			bool result = md.Run() == (int)ResponseType.Yes;
			md.Destroy();
			return result;
		}

		public string Question(string[] buttons, string message, string title = null)
		{
			throw new NotSupportedException("Текущая реализация сервиса дилогов пользователя не поддерживает создание диалогов с произвольными кнопками.");
		}
	}
}
