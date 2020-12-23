using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels;

namespace Vodovoz.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class UserView : TabViewBase<UserViewModel>
	{
		public UserView(UserViewModel viewModel) : base(viewModel)
		{
			this.Build();

			//ConfigureDlg();
		}
	}
}
