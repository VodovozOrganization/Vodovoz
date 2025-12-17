using System;
using System.Linq;
using Gamma.Binding;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Utilities;
using QS.Views.GtkUI;
using Vodovoz.Domain.Suppliers;
using Vodovoz.ViewModels.Suppliers;

namespace Vodovoz.Views.Suppliers
{
	public partial class RequestToSupplierView : TabViewBase<RequestToSupplierViewModel>
	{
		public RequestToSupplierView(RequestToSupplierViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			lblMsg.Binding
				.AddBinding(ViewModel, s => s.NeedRefresh, w => w.Visible)
				.InitializeFromSource();

			entName.Binding
				.AddBinding(ViewModel.Entity, s => s.Name, w => w.Text)
				.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			enumStatus.ItemsEnum = typeof(RequestStatus);
			enumStatus.Binding
				.AddBinding(ViewModel.Entity, vm => vm.Status, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			enumCmbSuppliersOrdering.ItemsEnum = typeof(SupplierOrderingType);
			enumCmbSuppliersOrdering.Binding
				.AddBinding(ViewModel.Entity, s => s.SuppliersOrdering, w => w.SelectedItem)
				.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			enumCmbSuppliersOrdering.EnumItemSelected += (sender, e) => ViewModel.RefreshCommand.Execute();

			chkDelayDays.Binding
				.AddBinding(ViewModel.Entity, vm => vm.WithDelayOnly, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			txtComment.Binding
				.AddBinding(ViewModel.Entity, s => s.Comment, w => w.Buffer.Text)
				.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
			
			ConfigureTree();

			ViewModel.ListContentChanged += (sender, e) => {
				treeItems.YTreeModel.EmitModelChanged();
				treeItems.ExpandAll();
			};

			btnOpenItem.Clicked += (sender, e) => ViewModel.OpenItemCommand.Execute(GetSelectedTreeItems());
			btnOpenItem.Binding
				.AddFuncBinding(ViewModel, vm => vm.AreNomenclatureNodesSelected || vm.AreSupplierNodesSelected, w => w.Sensitive)
				.InitializeFromSource();

			lblMinimalTotalSum.Binding
				.AddBinding(ViewModel, s => s.MinimalTotalSumText, w => w.Text)
				.InitializeFromSource();

			btnRefresh.Clicked += (sender, e) => {
				ViewModel.RefreshCommand.Execute();
				UpdateSensitivitiesOnSelectionChanged(sender, e);
			};
			btnRefresh.Binding
				.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			btnAdd.Clicked += (sender, e) => ViewModel.AddRequestingNomenclatureCommand.Execute();
			btnAdd.Binding
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			btnRemove.Clicked += (sender, e) => ViewModel.RemoveRequestingNomenclatureCommand.Execute(GetSelectedTreeItems());
			btnRemove.Binding
				.AddFuncBinding(ViewModel, vm => vm.AreNomenclatureNodesSelected && vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			btnTransfer.Clicked += (sender, e) => ViewModel.TransferRequestingNomenclatureCommand.Execute(GetSelectedTreeItems());
			btnTransfer.Binding
				.AddFuncBinding(ViewModel, vm => vm.AreNomenclatureNodesSelected && vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();

			btnSave.Clicked += (sender, e) => ViewModel.SaveAndClose();
			btnSave.Sensitive = ViewModel.CanEdit;
			btnCancel.Clicked += (sender, e) => ViewModel.Close(ViewModel.AskSaveOnClose, QS.Navigation.CloseSource.Cancel);
		}

		private void ConfigureTree()
		{
			treeItems.Selection.Mode = SelectionMode.Multiple;
			treeItems.ColumnsConfig = FluentColumnsConfig<ILevelingRequestNode>.Create()
				.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.Supplier.Id.ToString() : n.Nomenclature.Id.ToString())
				.AddColumn("ТМЦ")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? string.Empty : n.Nomenclature.ShortOrFullName)
				.AddColumn("Кол-во")
					.AddNumericRenderer(n => n.Quantity)
					.Digits(2)
					.WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter(
						(c, n) =>
						{
							c.Editable = false;
							if(n is SupplierNode)
							{
								c.Text = string.Empty;
							}
							else if(ViewModel.CanEdit)
							{
								c.Editable = true;
							}
						}
					)
				.AddColumn("Ед.изм.")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode || n.Nomenclature.Unit == null ? string.Empty : n.Nomenclature.Unit.Name)
				.AddColumn("Поставщик")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.Supplier.Name : string.Empty)
				.AddColumn("Оплата")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.PaymentType.GetEnumTitle() : string.Empty)
				.AddColumn("Цена закупки")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.Price.ToShortCurrencyString() : string.Empty)
				.AddColumn("Цена продажи")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is RequestToSupplierItem ? (n as RequestToSupplierItem).NomenclaturePrice : string.Empty)
				.AddColumn("НДС")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.NomenclatureToBuy.GetActualVatRateVersion(DateTime.Now).VatRate.Name : string.Empty)
				.AddColumn("Условия")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.PaymentCondition.GetEnumTitle() : string.Empty)
				.AddColumn("Отсрочка")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => ViewModel.GenerateDelayDaysString(n))
				.AddColumn("Комментарий")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.Comment : string.Empty)
					.WrapWidth(300)
					.WrapMode(Pango.WrapMode.WordChar)
				.AddColumn("Изменено")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n is SupplierNode ? n.SupplierPriceItem.ChangingDate.ToString("G") : string.Empty)
				.AddColumn("")
				.Finish();
			treeItems.YTreeModel =
				new RecursiveTreeModel<ILevelingRequestNode>(ViewModel.Entity.ObservableLevelingRequestNodes, x => x.Parent, x => x.Children);
			treeItems.ExpandAll();
			
			treeItems.Selection.Changed += UpdateSensitivitiesOnSelectionChanged;
			treeItems.RowActivated += (o, args) => ViewModel.OpenItemCommand.Execute(GetSelectedTreeItems());
		}

		void UpdateSensitivitiesOnSelectionChanged(object sender, System.EventArgs e)
		{
			ViewModel.AreNomenclatureNodesSelected = GetSelectedTreeItems().All(i => i is RequestToSupplierItem);
			ViewModel.AreSupplierNodesSelected = GetSelectedTreeItems().All(i => i is SupplierNode);
		}

		ILevelingRequestNode[] GetSelectedTreeItems() => treeItems.GetSelectedObjects<ILevelingRequestNode>();
	}
}
