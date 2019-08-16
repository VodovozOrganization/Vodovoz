using Gamma.Binding;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Client;

namespace Vodovoz.Views.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SupplierPricesWidgetView : EntityWidgetViewBase<SupplierPricesWidgetViewModel>
	{
		public SupplierPricesWidgetView()
		{
			this.Build();
		}

		public SupplierPricesWidgetView(SupplierPricesWidgetViewModel viewModel) : base(viewModel)
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			yTreePrices.ColumnsConfig = FluentColumnsConfig<ISupplierPriceNode>.Create()
				.AddColumn("№")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.IsEditable ? string.Empty : n.PosNr)
				.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.IsEditable ? string.Empty : n.NomenclatureToBuy.Id.ToString())
				.AddColumn("ТМЦ")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.IsEditable ? string.Empty : n.NomenclatureToBuy.ShortOrFullName)
				.AddColumn("Ед.изм.")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.IsEditable ? string.Empty : n.NomenclatureToBuy.Unit.Name)
				.AddColumn("Оплата")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => !n.IsEditable ? string.Empty : n.PaymentType.GetEnumTitle())
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(n => n.Price).Digits(2).WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter(
						(c, n) => {
							c.Editable = false;
							if(n.IsEditable)
								c.Editable = true;
							else
								c.Text = string.Empty;
						}
					)
				.AddColumn("НДС")
					.AddEnumRenderer(n => n.VAT, true)
					.AddSetter(
						(c, n) => {
							c.Editable = false;
							if(n.IsEditable)
								c.Editable = true;
							else
								c.Text = string.Empty;
						}
					)
				.AddColumn("Условия")
					.AddEnumRenderer(n => n.PaymentCondition, true)
					.AddSetter(
						(c, n) => {
							c.Editable = false;
							if(n.IsEditable)
								c.Editable = true;
							else
								c.Text = string.Empty;
						}
					)
				.AddColumn("Получение")
					.AddEnumRenderer(n => n.DeliveryType, true)
					.AddSetter(
						(c, n) => {
							c.Editable = false;
							if(n.IsEditable)
								c.Editable = true;
							else
								c.Text = string.Empty;
						}
					)
				.AddColumn("Комментарий")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Comment)
					.AddSetter(
						(c, n) => {
							c.Editable = false;
							if(n.IsEditable)
								c.Editable = true;
							else
								c.Text = string.Empty;
						}
					)
				.AddColumn("Статус")
					.AddEnumRenderer(n => n.AvailabilityForSale, true)
					.AddSetter(
						(c, n) => {
							c.Editable = false;
							if(n.IsEditable)
								c.Editable = true;
							else
								c.Text = string.Empty;
						}
					)
				.AddColumn("Изменено")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => !n.IsEditable ? string.Empty : n.ChangingDate.ToString("G"))
				.AddColumn("")
				.Finish();

			yTreePrices.YTreeModel = new RecursiveTreeModel<ISupplierPriceNode>(ViewModel.Entity.ObservablePriceNodes, x => x.Parent, x => x.Children);
			yTreePrices.ExpandAll();

			btnAdd.Binding.AddBinding(ViewModel, s => s.CanAdd, w => w.Sensitive).InitializeFromSource();
			btnAdd.Clicked += (s, ea) => ViewModel.AddItemCommand.Execute();

			btnEdit.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();
			btnEdit.Clicked += (s, ea) => ViewModel.AddItemCommand.Execute();

			btnDelete.Binding.AddBinding(ViewModel, s => s.CanRemove, w => w.Sensitive).InitializeFromSource();
			btnDelete.Clicked += (s, ea) => ViewModel.AddItemCommand.Execute();
		}
	}
}
