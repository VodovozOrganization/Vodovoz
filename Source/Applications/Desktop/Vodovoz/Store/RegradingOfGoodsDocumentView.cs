using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Store;

namespace Vodovoz
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
			regradingofgoodsitemsview.NavigationManager = ViewModel.NavigationManager;
			regradingofgoodsitemsview.ParrentDlg = this;

			regradingofgoodsitemsview.Sensitive = ViewModel.IsEditing;

			ylabelDate.Binding.AddFuncBinding(ViewModel.Entity, e => e.TimeStamp.ToString("g"), w => w.LabelProp).InitializeFromSource();

			regradingofgoodsitemsview.DocumentUoW = ViewModel.UoWGeneric;

			entityentryWarehouse.ViewModel = ViewModel.WarehouseViewModel;
			
			ytextviewCommnet.Binding.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			if(!ViewModel.Entity.CanEdit && ViewModel.Entity.TimeStamp.Date != DateTime.Now.Date)
			{
				ytextviewCommnet.Binding.AddFuncBinding(ViewModel.Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				entityentryWarehouse.Binding.AddFuncBinding(ViewModel.Entity, e => e.CanEdit, w => w.Sensitive).InitializeFromSource();
				regradingofgoodsitemsview.Sensitive = false;

				buttonSave.Sensitive = false;
			}
		}
	}
}
