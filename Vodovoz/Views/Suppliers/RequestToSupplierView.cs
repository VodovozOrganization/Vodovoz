using Gamma.Binding;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Dialog.GtkUI;
using QS.Utilities;
using QS.Views.GtkUI;
using Vodovoz.Domain.Suppliers;
using Vodovoz.ViewModels.Suppliers;

namespace Vodovoz.Views.Suppliers
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RequestToSupplierView : TabViewBase<RequestToSupplierViewModel>
	{
		public RequestToSupplierView(RequestToSupplierViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			lblMsg.Binding.AddBinding(ViewModel, s => s.NeedRefresh, w => w.Visible).InitializeFromSource();

			entName.Binding.AddBinding(ViewModel.Entity, s => s.Name, w => w.Text).InitializeFromSource();
			entName.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();

			enumCmbSuppliersOrdering.ItemsEnum = typeof(SupplierOrderingType);
			enumCmbSuppliersOrdering.Binding.AddBinding(ViewModel.Entity, s => s.SuppliersOrdering, w => w.SelectedItem).InitializeFromSource();
			enumCmbSuppliersOrdering.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();
			enumCmbSuppliersOrdering.EnumItemSelected += (sender, e) => ViewModel.RefreshCommand.Execute();

			txtComment.Binding.AddBinding(ViewModel.Entity, s => s.Comment, w => w.Buffer.Text).InitializeFromSource();
			txtComment.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();

			treeItems.ColumnsConfig = FluentColumnsConfig<ILevelingRequestNode>.Create()
				.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.Supplier.Id.ToString() : n.Nomenclature.Id.ToString())
				.AddColumn("ТМЦ")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? string.Empty : n.Nomenclature.ShortOrFullName)
				.AddColumn("Кол-во")
					.AddNumericRenderer(n => n.Quantity)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter(
						(c, n) => {
							c.Editable = false;
							if(n is SupplierNode)
								c.Text = string.Empty;
							else
								c.Editable = true;
						}
					)
				.AddColumn("Ед.изм.")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? string.Empty : n.Nomenclature.Unit.Name)
				.AddColumn("Поставщик")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.Supplier.Name : string.Empty)
				.AddColumn("Оплата")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.PaymentType.GetEnumTitle() : string.Empty)
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.Price.ToShortCurrencyString() : string.Empty)
				.AddColumn("НДС")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.VAT.GetEnumTitle() : string.Empty)
				.AddColumn("Условия")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.PaymentCondition.GetEnumTitle() : string.Empty)
				.AddColumn("Отсрочка")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => GenerateDelayDaysString(n))
				.AddColumn("Комментарий")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.Comment : string.Empty)
				.AddColumn("Изменено")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.ChangingDate.ToString("G") : string.Empty)
				.AddColumn("")
				.Finish();
			treeItems.YTreeModel = new RecursiveTreeModel<ILevelingRequestNode>(ViewModel.Entity.ObservableLevelingRequestNodes, x => x.Parent, x => x.Children);
			treeItems.ExpandAll();

			ViewModel.ListContentChanged += (sender, e) => {
				treeItems.YTreeModel.EmitModelChanged();
				treeItems.ExpandAll();
			};

			ViewModel.SupplierPricesUpdated += (sender, e) => {
				var response = MessageDialogHelper.RunQuestionWithTitleDialog(
					"Обновить список цен поставщиков?",
					"Цены на некоторые ТМЦ, выбранные в список цен поставщиков, изменились.\nЖелаете обновить список?"
				);
				if(response)
					ViewModel.RefreshCommand.Execute();
			};

			treeItems.Selection.Changed += (sender, e) => ViewModel.CanRemove = GetSelectedTreeItem() is RequestToSupplierItem;

			btnRefresh.Clicked += (sender, e) => ViewModel.RefreshCommand.Execute();
			btnRefresh.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();

			btnAdd.Clicked += (sender, e) => ViewModel.AddRequestingNomenclatureCommand.Execute();
			btnAdd.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();

			btnRemove.Clicked += (sender, e) => ViewModel.RemoveRequestingNomenclatureCommand.Execute(GetSelectedTreeItem());
			btnRemove.Binding.AddBinding(ViewModel, s => s.CanRemove, w => w.Sensitive).InitializeFromSource();

			btnTransfer.Clicked += (sender, e) => ViewModel.TransferRequestingNomenclatureCommand.Execute();
			btnTransfer.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();

			btnSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			btnCancel.Clicked += (sender, e) => ViewModel.Close(false);
		}

		string GenerateDelayDaysString(ILevelingRequestNode n)
		{
			if(n is SupplierNode)
				return n.SupplierPriceItem.Supplier.DelayDays > 0 ? string.Format("{0} дн.", n.SupplierPriceItem.Supplier.DelayDays) : "Нет";
			return string.Empty;
		}

		ILevelingRequestNode GetSelectedTreeItem() => treeItems.GetSelectedObject<ILevelingRequestNode>();
	}
}
