using Autofac;
using Gtk;
using QS.Dialog;
using System;
using Vodovoz.Application.Pacs;

public partial class MainWindow
{
	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		var canPacsClose = _operatorService.CanStopApplication();
		if (!canPacsClose)
		{
			_interativeService.ShowMessage(ImportanceLevel.Warning, "Вы не завершили смену.");
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
		var canPacsClose = _operatorService.CanStopApplication();
		if(!canPacsClose)
		{
			_interativeService.ShowMessage(ImportanceLevel.Warning, "Вы не завершили смену.");
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
