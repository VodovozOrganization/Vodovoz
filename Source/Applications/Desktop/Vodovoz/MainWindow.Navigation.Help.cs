using QS.Project.ViewModels;
using QS.Project.Views;
using System;

public partial class MainWindow
{
	/// <summary>
	/// О программе
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnAboutActionActivated(object sender, EventArgs e)
	{
		var aboutViewModel = new AboutViewModel(_applicationInfo);
		var aboutView = new AboutView(aboutViewModel);
		aboutView.ShowAll();
		aboutView.Run();
		aboutView.Destroy();
	}
}
