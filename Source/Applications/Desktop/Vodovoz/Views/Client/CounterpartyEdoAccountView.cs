using System;
using System.ComponentModel;

namespace Vodovoz.Views.Client
{
	[ToolboxItem(true)]
	public partial class CounterpartyEdoAccountView : Gtk.Bin
	{
		public CounterpartyEdoAccountView()
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			var accountView = new EdoAccountView();
			vboxEdoAccountsByOrganization.Add(accountView);
			accountView.Show();
		}
	}
}
