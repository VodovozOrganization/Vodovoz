using System;
using Gamma.Binding;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QS.Project.Search;
using QS.Project.Search.GtkUI;
using QS.Utilities;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Client;

namespace Vodovoz.Views.Client
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SupplierPricesWidgetView : WidgetViewBase<SupplierPricesWidgetViewModel>
	{
		public SupplierPricesWidgetView()
		{
			Build();
		}

		public SupplierPricesWidgetView(SupplierPricesWidgetViewModel viewModel) : base(viewModel)
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			var searchView = new SearchView(ViewModel.Search as SearchViewModel);
			hboxSearch.Add(searchView);
			searchView.Show();

			spinDelayDays.Binding.AddBinding(ViewModel.Entity, s => s.DelayDaysForProviders, w => w.ValueAsInt).InitializeFromSource();
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
					.AddTextRenderer(n => n.IsEditable || n.NomenclatureToBuy.Unit == null ? string.Empty : n.NomenclatureToBuy.Unit.Name)
				.AddColumn("Оплата")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => !n.IsEditable ? string.Empty : n.PaymentType.GetEnumTitle())
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(n => n.Price).Digits(4).WidthChars(10)
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
					.AddTextRenderer(n => n.IsEditable ? CurrencyWorks.CurrencyShortName : string.Empty)
				.AddColumn("НДС")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.NomenclatureToBuy.GetActualVatRateVersion(DateTime.Now).VatRate.Name)
				.AddColumn("Условия")
					.HeaderAlignment(0.5f)
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
					.HeaderAlignment(0.5f)
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
						.WrapWidth(300)
						.WrapMode(Pango.WrapMode.WordChar)
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
					.HeaderAlignment(0.5f)
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
					.AddTextRenderer(n => !n.IsEditable || n.Id <= 0 ? string.Empty : n.ChangingDate.ToString("G"))
				.AddColumn("")
				.Finish();

			yTreePrices.YTreeModel = new RecursiveTreeModel<ISupplierPriceNode>(ViewModel.Entity.ObservablePriceNodes, x => x.Parent, x => x.Children);
			yTreePrices.ExpandAll();

			ViewModel.ListContentChanged += (sender, e) => {
				yTreePrices.YTreeModel.EmitModelChanged();
				yTreePrices.ExpandAll();
			};
			yTreePrices.Selection.Changed += (sender, e) => ViewModel.CanRemove = GetSelectedTreeItem() != null;

			btnAdd.Binding.AddBinding(ViewModel, s => s.CanAdd, w => w.Sensitive).InitializeFromSource();
			btnAdd.Clicked += (s, ea) => ViewModel.AddItemCommand.Execute();

			btnEdit.Binding.AddBinding(ViewModel, s => s.CanEdit, w => w.Visible).InitializeFromSource();
			btnEdit.Clicked += (s, ea) => ViewModel.AddItemCommand.Execute();

			btnDelete.Binding.AddBinding(ViewModel, s => s.CanRemove, w => w.Sensitive).InitializeFromSource();
			btnDelete.Clicked += (s, ea) => ViewModel.RemoveItemCommand.Execute(GetSelectedTreeItem());
		}

		ISupplierPriceNode GetSelectedTreeItem() => yTreePrices.GetSelectedObject<ISupplierPriceNode>();

		protected override void OnDestroyed()
		{
			ViewModel.Dispose();
			base.OnDestroyed();
		}
	}
}
