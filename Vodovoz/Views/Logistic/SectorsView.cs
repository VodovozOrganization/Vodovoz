using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SectorsView : TabViewBase<SectorsViewModel>, ISingleUoWDialog
	{
		public SectorsView(SectorsViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}

		public IUnitOfWork UoW { get; set; }
	}
}
