using System;
using Vodovoz.ViewModels;

public partial class MainWindow
{
	/// <summary>
	/// О программе
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAboutActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<AboutViewModel>(null);
	}
}
