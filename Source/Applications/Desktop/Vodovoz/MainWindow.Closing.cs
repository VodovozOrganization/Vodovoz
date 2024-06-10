using Gtk;
using System;
using Vodovoz.Presentation.ViewModels.Pacs;

public partial class MainWindow
{
	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		var isOperaorShiftActive = _operatorService.IsOperatorShiftActive();

		if (isOperaorShiftActive && _interativeService.Question(
			"Завершить смену?",
			"Смена не завершена!!"))
		{
			NavigationManager.OpenViewModel<PacsViewModel>(null);
			a.RetVal = true;
			return;
		}

		if (tdiMain.CloseAllTabs())
		{
			a.RetVal = false;
			Close();
		}
		else
		{
			a.RetVal = true;
		}
	}

	/// <summary>
	/// Выход
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnQuitActionActivated(object sender, EventArgs e)
	{
		var isOperaorShiftActive = _operatorService.IsOperatorShiftActive();

		if(isOperaorShiftActive && _interativeService.Question(
			"Завершить смену?",
			"Смена не завершена!!"))
		{
			NavigationManager.OpenViewModel<PacsViewModel>(null);
			return;
		}

		if (tdiMain.CloseAllTabs())
		{
			Close();
		}
	}

	private void Close()
	{
		_autofacScope.Dispose();
		Application.Quit();
	}
}
