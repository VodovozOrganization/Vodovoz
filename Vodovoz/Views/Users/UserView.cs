using System;
using QS.Views.GtkUI;
using Vodovoz.Dialogs.Users;
namespace Vodovoz.Views.Users
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserView : TabViewBase<UserViewModel>
	{
		public UserView(UserViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ConfigureDialog();
		}

		private void ConfigureDialog()
		{

		}
	}
}
