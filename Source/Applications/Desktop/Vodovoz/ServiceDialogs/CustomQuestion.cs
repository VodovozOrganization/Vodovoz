using System;
using Gtk;
using QS.Dialog;
using Vodovoz.Dialogs;

namespace Vodovoz.ServiceDialogs
{
	public class CustomQuestion : IInteractiveQuestion
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
