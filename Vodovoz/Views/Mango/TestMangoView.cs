using System;
using QS.Views.Dialog;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.ViewModels.Mango;
using Vodovoz.Domain.Orders;
using Gamma.GtkWidgets;
using Gtk;
using Vodovoz.Domain.Client;
using FluentNHibernate.Data;
using System.Collections.Generic;

namespace Vodovoz.Views.Mango
{
	public partial class TestMangoView : DialogViewBase<TestMangoViewModel>
		{
		public TestMangoView(TestMangoViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}

		protected void Clicked_GetAllVPBXEmploies(object sender, EventArgs e)
		{
			ViewModel.GetAllVPBXEmploies();
		}

		protected void Clicked_HangUp(object sender, EventArgs e)
		{
			ViewModel.HangUp();
		}
	}
}
