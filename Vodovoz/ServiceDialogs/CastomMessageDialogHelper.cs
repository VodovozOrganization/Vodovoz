using Gtk;
using Vodovoz.Dialogs;

namespace Vodovoz.Services
{
	public static class CastomMessageDialogHelper
	{
		public static bool RunQuestionDialog(string question, params object[] args)
		{
			return RunQuestionDialog(string.Format(question, args));
		}

		public static bool RunQuestionDialog(string question)
		{
			CastomMessageDlg md = new CastomMessageDlg(null,
								   DialogFlags.Modal,
								   MessageType.Question,
								   ButtonsType.YesNo,
								   question);

			md.SetPosition(WindowPosition.Center);			
			bool result = md.Run() == (int)ResponseType.Yes;
			md.Destroy();
			return result;
		}

		public static bool RunQuestionWithTitleDialog(string title, string question)
		{
			CastomMessageDlg md = new CastomMessageDlg(null,
								   DialogFlags.Modal,
								   MessageType.Question,
								   ButtonsType.YesNo,
								   question);

			md.SetPosition(WindowPosition.Center);
			md.Title = title;
			bool result = md.Run() == (int)ResponseType.Yes;
			md.Destroy();
			return result;
		}

		public static int RunQuestionYesNoCancelDialog(string question, string title = null)
		{
			CastomMessageDlg md = new CastomMessageDlg(null,
				DialogFlags.Modal,
				MessageType.Question,
				ButtonsType.None,
				question);

			md.AddButton("Да", ResponseType.Yes);
			md.AddButton("Нет", ResponseType.No);
			md.AddButton("Отмена", ResponseType.Cancel);
			md.SetPosition(WindowPosition.Center);
			md.Title = title;
			int result = md.Run();
			md.Destroy();
			return result;
		}

		public static void RunWarningDialog(string warning, string title = null)
		{
			CastomMessageDlg md = new CastomMessageDlg(null,
								   DialogFlags.Modal,
								   MessageType.Warning,
								   ButtonsType.Ok,
								   warning);

			md.SetPosition(WindowPosition.Center);
			md.Title = title ?? "Предупреждение";
			md.Run();
			md.Destroy();
		}

		public static bool RunWarningDialog(string title, string warning, ButtonsType buttons = ButtonsType.YesNo)
		{
			CastomMessageDlg md = new CastomMessageDlg(null,
								   DialogFlags.Modal,
								   MessageType.Warning,
								   buttons,
								   warning);

			md.SetPosition(WindowPosition.Center);
			md.Title = title;
			bool result = md.Run() == (int)ResponseType.Yes;
			md.Destroy();
			return result;
		}

		public static void RunErrorDialog(string formattedError, params object[] args)
		{
			RunErrorDialog(string.Format(formattedError, args));
		}

		public static void RunErrorDialog(string error, string title = null)
		{
			RunErrorDialog(true, error, title);
		}

		public static void RunErrorDialog(bool useMarkup, string error, string title = null)
		{
			CastomMessageDlg md = new CastomMessageDlg(null,
				DialogFlags.Modal,
				MessageType.Error,
				ButtonsType.Ok,
				error,
				useMarkup);

			md.SetPosition(WindowPosition.Center);
			md.Title = title ?? "Ошибка";
			md.Run();
			md.Destroy();
		}

		public static void RunInfoDialog(string formattedMessage, params object[] args)
		{
			RunInfoDialog(string.Format(formattedMessage, args));
		}

		public static void RunInfoDialog(string message, string title = null)
		{
			CastomMessageDlg md = new CastomMessageDlg(null,
				DialogFlags.Modal,
				MessageType.Info,
				ButtonsType.Ok,
				message);

			md.SetPosition(WindowPosition.Center);
			md.Title = title ?? "Информация";
			md.Run();
			md.Destroy();
		}
	}
}
