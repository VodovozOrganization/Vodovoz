using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.ViewModels.Employees;
namespace Vodovoz.Views.Employees
{
	[ToolboxItem(true)]
	public partial class FineCategoryView : TabViewBase<FineCategoryViewModel>
	{
		public FineCategoryView(FineCategoryViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			throw new NotImplementedException();
		}
	}
}
