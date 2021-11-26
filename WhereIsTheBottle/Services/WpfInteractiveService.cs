using System;
using System.Windows;
using QS.Dialog;

namespace WhereIsTheBottle.Services
{
	public class WpfInteractiveService : IInteractiveService
	{
		private readonly MessageWindowViewModel _messageWindowViewModel;

		public WpfInteractiveService()
		{
			_messageWindowViewModel = new MessageWindowViewModel();
		}

		public void ShowMessage(ImportanceLevel level, string message, string title = null)
		{
			_messageWindowViewModel.Message = message;
			_messageWindowViewModel.Title = title ?? GetDefaultTitle(level);
			_messageWindowViewModel.ImportanceLevel = level;

			var messageWindowView = new MessageWindowView(_messageWindowViewModel)
				{ WindowStartupLocation = WindowStartupLocation.CenterScreen, ShowInTaskbar = false };
			var currentMainWindow = Application.Current.MainWindow;
			if(currentMainWindow != null && currentMainWindow != messageWindowView)
			{
				messageWindowView.Owner = currentMainWindow;
			}

			messageWindowView.Closed += MessageWindowViewOnClosed;
			messageWindowView.ShowDialog();
			messageWindowView.Closed -= MessageWindowViewOnClosed;
		}

		private string GetDefaultTitle(ImportanceLevel level)
		{
			return level switch
			{
				ImportanceLevel.Info => "Информация",
				ImportanceLevel.Warning => "Предупреждение",
				ImportanceLevel.Error => "Ошибка",
				_ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
			};
		}

		private void MessageWindowViewOnClosed(object sender, EventArgs e)
		{
			((Window)sender).Owner?.Activate();
		}

		public bool Question(string message, string title = null)
		{
			throw new NotImplementedException();
		}
	}
}
