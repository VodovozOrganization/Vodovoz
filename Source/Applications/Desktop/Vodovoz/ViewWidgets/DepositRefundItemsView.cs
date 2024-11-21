using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DepositRefundItemsView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		public IUnitOfWork UoW { get; set; }

		public INavigationManager NavigationManager { get; } = Startup.MainWin.NavigationManager;

		public Order Order { get; set; }

		/// <summary>
		/// Перезапись встроенного свойства Sensitive
		/// Sensitive теперь работает только с таблицей
		/// К сожалению Gtk обходит этот параметр, если выставлять Sensitive какому-либо элементу управления выше по дереву
		/// </summary>
		public new bool Sensitive
		{
			get => treeDepositRefundItems.Sensitive && hboxDeposit.Sensitive;
			set => treeDepositRefundItems.Sensitive = hboxDeposit.Sensitive = value;
		}

		public DepositRefundItemsView() => this.Build();

		public void Configure(IUnitOfWork uow, Order order, bool scrolled = false)
		{
			Order = order;
			this.UoW = uow;

			treeDepositRefundItems.ColumnsConfig = ColumnsConfigFactory.Create<OrderDepositItem>()
				.AddColumn("Тип")
					.AddTextRenderer(node => node.DepositTypeString)
				.AddColumn("Название")
					.AddTextRenderer(node => node.EquipmentNomenclature != null ? node.EquipmentNomenclature.Name : string.Empty)
				.AddColumn("Кол-во")
					.AddNumericRenderer(node => node.Count)
						.Adjustment(new Adjustment(1, 0, 100000, 1, 100, 1))
						.Editing(!(MyTab is OrderReturnsView))
				.AddColumn("Факт. кол-во")
					.AddNumericRenderer(node => node.ActualCount, new NullValueToZeroConverter())
						.Adjustment(new Adjustment(1, 0, 100000, 1, 100, 1))
						.Editing(MyTab is OrderReturnsView)
						.AddColumn("Цена")
					.AddNumericRenderer(node => node.Deposit)
						.Adjustment(new Adjustment(1, 0, 1000000, 1, 100, 1))
						.WidthChars(10)
						.Editing(true)
				.AddColumn("Сумма")
					.AddNumericRenderer(node => node.ActualSum)
				.Finish();

			treeDepositRefundItems.ItemsDataSource = Order.ObservableOrderDepositItems;
			treeDepositRefundItems.Selection.Changed += TreeDepositRefundItems_Selection_Changed;

			scrolledwindow2.VscrollbarPolicy = scrolled ? PolicyType.Always : PolicyType.Never;
		}

		protected void OnButtonNewBottleDepositClicked(object sender, EventArgs e)
		{
			OrderDepositItem newDepositItem = new OrderDepositItem {
				Count = MyTab is OrderReturnsView ? 0 : 1,
				ActualCount = null,
				Order = Order,
				DepositType = DepositType.Bottles
			};
			Order.ObservableOrderDepositItems.Add(newDepositItem);
		}

		protected void OnButtonDeleteDepositClicked(object sender, EventArgs e)
		{
			if(treeDepositRefundItems.GetSelectedObject() is OrderDepositItem depositItem)
				if(MyTab is OrderReturnsView) {
					//Удаление только новых залогов добавленных из закрытия МЛ
					if(depositItem.Count == 0)
						Order.ObservableOrderDepositItems.Remove(depositItem);
				} else
					Order.ObservableOrderDepositItems.Remove(depositItem);
		}

		protected void OnButtonNewEquipmentDepositClicked(object sender, EventArgs e)
		{
			(NavigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(this.MyTab, filter =>
				{
					filter.RestrictCategory = NomenclatureCategory.equipment;
				},
				OpenPageOptions.AsSlave,
				viewModel => {
					viewModel.OnSelectResult += Journal_OnEntitySelectedResult;
					viewModel.Title = "Оборудование";
					viewModel.SelectionMode = JournalSelectionMode.Single;
				});
		}

		private void Journal_OnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var selectedNode = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();
			if(selectedNode == null)
			{
				return;
			}

			var selectedNomenclature = UoW.GetById<Nomenclature>(selectedNode.Id);
			OrderDepositItem newDepositItem = new OrderDepositItem
			{
				Count = 0,
				ActualCount = null,
				Order = Order,
				EquipmentNomenclature = selectedNomenclature,
				DepositType = DepositType.Equipment
			};
			Order.ObservableOrderDepositItems.Add(newDepositItem);
		}

		void TreeDepositRefundItems_Selection_Changed(object sender, EventArgs e)
		{
			object[] items = treeDepositRefundItems.GetSelectedObjects();
			buttonDeleteDeposit.Sensitive = items.Any();
		}
	}
}
