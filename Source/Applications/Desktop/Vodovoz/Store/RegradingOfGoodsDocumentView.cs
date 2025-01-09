using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Store;

namespace Vodovoz.Store
{
	public partial class RegradingOfGoodsDocumentView : TabViewBase<RegradingOfGoodsDocumentViewModel>
	{
		public RegradingOfGoodsDocumentView(RegradingOfGoodsDocumentViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			regradingofgoodsitemsview.ViewModel = ViewModel.ItemsViewModel;

			regradingofgoodsitemsview.Sensitive = ViewModel.CanEditItems;

			ylabelDate.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp)
				.InitializeFromSource();

			entityentryWarehouse.ViewModel = ViewModel.WarehouseViewModel;
			
			ytextviewCommnet.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			if(!ViewModel.Entity.CanEdit
				&& ViewModel.Entity.TimeStamp.Date != DateTime.Now.Date)
			{
				ytextviewCommnet.Binding
					.AddFuncBinding(ViewModel.Entity, e => e.CanEdit, w => w.Sensitive)
					.InitializeFromSource();

				entityentryWarehouse.Binding
					.AddFuncBinding(ViewModel.Entity, e => e.CanEdit, w => w.Sensitive)
					.InitializeFromSource();
			}

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CancelCommand);
		}
	}
}
