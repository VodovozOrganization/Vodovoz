using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Client;
using Gamma.Utilities;
using Gtk;

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
			yTreePrices.ColumnsConfig = FluentColumnsConfig<SuplierPriceItem>.Create()
				.AddColumn("№")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => "?")
				.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
				.AddColumn("ТМЦ")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.NomenclatureToBuy.ShortOrFullName)
				.AddColumn("Ед.изм.")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.NomenclatureToBuy.Unit.Name)
				.AddColumn("Оплата")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.PaymentType.GetEnumTitle())
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(n => n.Price).Digits(2).WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.Editing(true)
				.AddColumn("Принадлежность")
					.AddEnumRenderer(n => n.VAT, true)
					.Editing()
				.AddColumn("Условия")
					.AddEnumRenderer(n => n.PaymentCondition, true)
					.Editing()
				.AddColumn("Получение")
					.AddEnumRenderer(n => n.DeliveryType, true)
					.Editing()
				.AddColumn("Комментарий")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Comment)
					.Editable()
				.AddColumn("Статус")
					.AddEnumRenderer(n => n.AvailabilityForSale, true)
					.Editing()
				.AddColumn("Изменено")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.ChangingDate.ToString("G"))
				.AddColumn("")
				.Finish();
			yTreePrices.Binding.AddBinding(ViewModel.Entity, s => s.ObservableSuplierPriceItems, w => w.ItemsDataSource).InitializeFromSource();
			//yTreePrices.Selection.Changed += (sender, e) => ViewModel.CanRemoveGuilty = GetSelectedGuilty() != null;

			btnAdd.Binding.AddBinding(ViewModel, s => s.CanAdd, w => w.Sensitive).InitializeFromSource();
			btnAdd.Clicked += (s, ea) => ViewModel.AddItemCommand.Execute();

			btnEdit.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Sensitive).InitializeFromSource();
			btnEdit.Clicked += (s, ea) => ViewModel.AddItemCommand.Execute();

			btnDelete.Binding.AddBinding(ViewModel, s => s.CanRemove, w => w.Sensitive).InitializeFromSource();
			btnDelete.Clicked += (s, ea) => ViewModel.AddItemCommand.Execute();
		}
	}
}
