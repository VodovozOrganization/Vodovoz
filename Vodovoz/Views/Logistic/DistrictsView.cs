using System;
using Gamma.ColumnConfig;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DistrictsView : TabViewBase<DistrictViewModel>, ISingleUoWDialog
	{
		public DistrictsView(DistrictViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}

		public IUnitOfWork UoW { get; set; }
	}
}
