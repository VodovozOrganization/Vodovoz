using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using System;
using System.Collections.Generic;
using System.Linq;
using QS.Navigation;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.ViewWidgets
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderEquipmentItemsView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		private IList<int> _activeFlyersNomenclaturesIds;
		private IFlyerRepository _flyerRepository;
		public IUnitOfWork UoW { get; set; }

		public Order Order { get; set; }

		public event EventHandler<OrderEquipment> OnDeleteEquipment;

		public OrderEquipmentItemsView()
		{
			this.Build();
		}

		/// <summary>
		/// Перезапись встроенного свойства Sensitive
		/// Sensitive теперь работает только с таблицей
		/// К сожалению Gtk обходит этот параметр, если выставлять Sensitive какому-либо элементу управления выше по дереву
		/// </summary>
		public new bool Sensitive
		{
			get => treeEquipment.Sensitive && hboxButtons.Sensitive;
			set => treeEquipment.Sensitive = hboxButtons.Sensitive = value;
		}

		/// <summary>
		/// Ширина первой колонки списка оборудования (создано для храннения 
		/// ширины колонки до автосайза ячейки по содержимому, чтобы отобразить
		/// по правильному положению ввод количества при добавлении нового товара)
		/// </summary>
		int treeAnyGoodsFirstColWidth;

		public void Configure(IUnitOfWork uow, Order order, IFlyerRepository flyerRepository)
		{
			_flyerRepository = flyerRepository ?? throw new ArgumentNullException(nameof(flyerRepository));
			
			UoW = uow;
			Order = order;
			UpdateActiveFlyersNomenclaturesIds();

			buttonDeleteEquipment.Sensitive = false;
			Order.ObservableOrderEquipments.ElementAdded += Order_ObservableOrderEquipments_ElementAdded;

			if(MyTab is OrderReturnsView) {
				SetColumnConfigForReturnView();
				lblEquipment.Visible = false;
			}
			else
				SetColumnConfigForOrderDlg();

			treeEquipment.ItemsDataSource = Order.ObservableOrderEquipments;
			treeEquipment.Selection.Changed += TreeEquipment_Selection_Changed;
		}

		public void UpdateActiveFlyersNomenclaturesIds()
		{
			_activeFlyersNomenclaturesIds = _flyerRepository.GetAllActiveFlyersNomenclaturesIdsByDate(UoW, Order.DeliveryDate);
		}

		public void UnsubscribeOnEquipmentAdd()
		{
			Order.ObservableOrderEquipments.ElementAdded -= Order_ObservableOrderEquipments_ElementAdded;
		}
		
		private void SetColumnConfigForOrderDlg()
		{
			var colorBlack = GdkColors.PrimaryText;
			var colorBlue = GdkColors.InfoBase;
			var colorGreen = GdkColors.SuccessBase;
			var colorWhite = GdkColors.PrimaryBase;
			var colorLightYellow = GdkColors.WarningBase;
			var colorLightRed = GdkColors.DangerBase;

			treeEquipment.ColumnsConfig = ColumnsConfigFactory.Create<OrderEquipment>()
				.AddColumn("Наименование").AddTextRenderer(node => node.FullNameString)
				.AddColumn("Направление").AddTextRenderer(node => node.DirectionString)
				.AddColumn("Кол-во(недовоз)")
				.AddNumericRenderer(node => node.Count).WidthChars(10)
				.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
				.AddSetter((cell, node) => {
					cell.Editable = !_activeFlyersNomenclaturesIds.Contains(node.Nomenclature.Id)
					                && !(node.OrderItem != null && node.OwnType == OwnTypes.Rent);
				})
				.AddTextRenderer(node => $"({node.UndeliveredCount})")
				.AddColumn("Принадлежность").AddEnumRenderer(node => node.OwnType, true, new Enum[] { OwnTypes.None })
				.AddSetter((c, n) => {
					c.Editable = false;
					c.Editable = n.Nomenclature?.Category == NomenclatureCategory.equipment;
				})
				.AddSetter((c, n) => {
					c.BackgroundGdk = colorWhite;
					if(n.Nomenclature?.Category == NomenclatureCategory.equipment
					  && n.OwnType == OwnTypes.None) {
						c.BackgroundGdk = colorLightRed;
					}
				})
				.AddColumn("Причина").AddEnumRenderer(
					node => node.DirectionReason
					, true
				)
				.HideCondition(HideItemFromDirectionReasonComboInEquipment)
				.AddSetter((c, n) => {
					if(n.Direction == Core.Domain.Orders.Direction.Deliver) {
						switch(n.DirectionReason) {
							case DirectionReason.Rent:
								c.Text = "В аренду";
								break;
							case DirectionReason.Repair:
								c.Text = "Из ремонта";
								break;
							case DirectionReason.Cleaning:
								c.Text = "После санобработки";
								break;
							case DirectionReason.RepairAndCleaning:
								c.Text = "Из ремонта и санобработки";
								break;
							default:
								break;
						}
					} else {
						switch(n.DirectionReason) {
							case DirectionReason.Rent:
								c.Text = "Закрытие аренды";
								break;
							case DirectionReason.Repair:
								c.Text = "В ремонт";
								break;
							case DirectionReason.Cleaning:
								c.Text = "На санобработку";
								break;
							case DirectionReason.RepairAndCleaning:
								c.Text = "В ремонт и санобработку";
								break;
							case DirectionReason.TradeIn:
								c.Text = "По акции \"Трейд-Ин\"";
								break;
							case DirectionReason.ClientGift:
								c.Text = "Подарок от клиента";
								break;
							default:
								break;
						}
					}

					c.UpdateComboList(n);
				})
				.AddSetter((c, n) => {
					c.Editable = false;
					c.Editable =
					     n.Nomenclature?.Category == NomenclatureCategory.equipment
					     && n.Reason != Reason.Rent
					     && n.OwnType != OwnTypes.Duty
					     && n.Nomenclature?.SaleCategory != SaleCategory.forSale;
				})
				.AddSetter((c, n) => {
					c.BackgroundGdk = (n.Nomenclature?.Category == NomenclatureCategory.equipment
									   && n.DirectionReason == DirectionReason.None
				                       && n.OwnType != OwnTypes.Duty
				                       && n.Nomenclature?.SaleCategory != SaleCategory.forSale)
						? colorLightRed
						: colorWhite;
				})
				.AddColumn("")
				.Finish();
		}

		private void SetColumnConfigForReturnView()
		{
			var colorBlack = GdkColors.PrimaryText;
			var colorBlue = GdkColors.InfoBase;
			var colorGreen = GdkColors.SuccessBase;
			var colorWhite = GdkColors.PrimaryBase;
			var colorLightYellow = GdkColors.WarningBase;
			var colorLightRed = GdkColors.DangerBase;

			treeEquipment.ColumnsConfig = ColumnsConfigFactory.Create<OrderEquipment>()
				.AddColumn("Наименование").AddTextRenderer(node => node.FullNameString)
				.AddColumn("Направление").AddTextRenderer(node => node.DirectionString)
				.AddColumn("Кол-во(недовоз)")
				.AddNumericRenderer(node => node.Count).WidthChars(10)
				.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing(false)
				.AddTextRenderer(node => $"({node.UndeliveredCount})")
				.AddColumn("Кол-во по факту")
					.AddNumericRenderer(node => node.ActualCount, new NullValueToZeroConverter(), false)
					.AddSetter((cell, node) => {
						cell.Editable = false;
						foreach(var cat in Nomenclature.GetCategoriesForGoods()) {
							if(cat == node.Nomenclature.Category)
								cell.Editable = true;
						}
					})
					.Adjustment(new Gtk.Adjustment(0, 0, 9999, 1, 1, 0))
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? string.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Причина незабора").AddTextRenderer(x => x.ConfirmedComment)
				.AddSetter((cell, node) => cell.Editable = node.Direction == Core.Domain.Orders.Direction.PickUp)
				.AddColumn("Принадлежность").AddEnumRenderer(node => node.OwnType, true, new Enum[] { OwnTypes.None })
				.AddSetter((c, n) => {
					c.Editable = false;
					c.Editable = n.Nomenclature?.Category == NomenclatureCategory.equipment;
				})
				.AddSetter((c, n) => {
					c.BackgroundGdk = colorWhite;
					if(n.Nomenclature?.Category == NomenclatureCategory.equipment
					  && n.OwnType == OwnTypes.None) {
						c.BackgroundGdk = colorLightRed;
					}
				})
				.AddColumn("Причина").AddEnumRenderer(
					node => node.DirectionReason,
					true
				)
				.HideCondition(HideItemFromDirectionReasonComboInEquipment)
				.AddSetter((c, n) => {
					if(n.Direction == Core.Domain.Orders.Direction.Deliver) {
						switch(n.DirectionReason) {
							case DirectionReason.Rent:
								c.Text = "В аренду";
								break;
							case DirectionReason.Repair:
								c.Text = "Из ремонта";
								break;
							case DirectionReason.Cleaning:
								c.Text = "После санобработки";
								break;
							case DirectionReason.RepairAndCleaning:
								c.Text = "Из ремонта и санобработки";
								break;
							default:
								break;
						}
					} else {
						switch(n.DirectionReason) {
							case DirectionReason.Rent:
								c.Text = "Закрытие аренды";
								break;
							case DirectionReason.Repair:
								c.Text = "В ремонт";
								break;
							case DirectionReason.Cleaning:
								c.Text = "На санобработку";
								break;
							case DirectionReason.RepairAndCleaning:
								c.Text = "В ремонт и санобработку";
								break;
							case DirectionReason.TradeIn:
								c.Text = "По акции \"Трейд-Ин\"";
								break;
							case DirectionReason.ClientGift:
								c.Text = "Подарок от клиента";
								break;
							default:
								break;
						}
					}

					c.UpdateComboList(n);
				})
				.AddSetter((c, n) => {
					c.Editable = false;
					c.Editable =
						n.Nomenclature?.Category == NomenclatureCategory.equipment
					     && n.Reason != Reason.Rent
					     && n.OwnType != OwnTypes.Duty
					     && n.Nomenclature?.SaleCategory != SaleCategory.forSale;
				})
				.AddSetter((c, n) => {
					c.BackgroundGdk = (n.Nomenclature?.Category == NomenclatureCategory.equipment
									   && n.DirectionReason == DirectionReason.None
				                       && n.OwnType != OwnTypes.Duty
				                       && n.Nomenclature?.SaleCategory != SaleCategory.forSale)
						? colorLightRed
						: colorWhite;
				})
				.AddColumn("")
				.Finish();
		}

		public virtual bool HideItemFromDirectionReasonComboInEquipment(OrderEquipment node, DirectionReason item)
		{
			switch(item) {
				case DirectionReason.None:
					return true;
				case DirectionReason.Rent:
				case DirectionReason.Repair:
				case DirectionReason.Cleaning:
				case DirectionReason.RepairAndCleaning:
					return false;
				case DirectionReason.TradeIn:
				case DirectionReason.ClientGift:
					return node.Direction == Core.Domain.Orders.Direction.Deliver;
				default:
					return false;
			}
		}

		void TreeEquipment_Selection_Changed(object sender, EventArgs e)
		{
			object[] items = treeEquipment.GetSelectedObjects();

			if(!items.Any())
				return;

			buttonDeleteEquipment.Sensitive = items.Any();
		}

		void Order_ObservableOrderEquipments_ElementAdded(object aList, int[] aIdx)
		{
			treeAnyGoodsFirstColWidth = treeEquipment.Columns.First(x => x.Title == "Наименование").Width;
			treeEquipment.ExposeEvent += TreeAnyGoods_ExposeEvent;
			//Выполнение в случае если размер не поменяется
			EditGoodsCountCellOnAdd(treeEquipment);
		}

		void TreeAnyGoods_ExposeEvent(object o, ExposeEventArgs args)
		{
			var newColWidth = ((yTreeView)o).Columns.First().Width;
			if(treeAnyGoodsFirstColWidth != newColWidth) {
				EditGoodsCountCellOnAdd((yTreeView)o);
				((yTreeView)o).ExposeEvent -= TreeAnyGoods_ExposeEvent;
			}
		}

		/// <summary>
		/// Активирует редактирование ячейки количества
		/// </summary>
		private void EditGoodsCountCellOnAdd(yTreeView treeView)
		{
			int index = treeView.Model.IterNChildren() - 1;
			TreePath path;

			treeView.Model.IterNthChild(out TreeIter iter, index);
			path = treeView.Model.GetPath(iter);

			var column = treeView.Columns.First(x => x.Title == "Кол-во(недовоз)");
			var renderer = column.CellRenderers.First();
			Gtk.Application.Invoke(delegate {
				treeView.SetCursorOnCell(path, column, renderer, true);
			});
			treeView.GrabFocus();
		}

		protected void OnButtonDeleteEquipmentClicked(object sender, EventArgs e)
		{
			if(treeEquipment.GetSelectedObject() is OrderEquipment deletedEquipment) {
				OnDeleteEquipment?.Invoke(this, deletedEquipment);
				//при удалении номенклатуры выделение снимается и при последующем удалении exception
				//для исправления делаем кнопку удаления не активной, если объект не выделился в списке
				buttonDeleteEquipment.Sensitive = treeEquipment.GetSelectedObject() != null;
			}
		}

		protected void OnButtonAddEquipmentToClientClicked(object sender, EventArgs e)
		{
			if(Order.Client == null) {
				MessageDialogHelper.RunWarningDialog("Для добавления товара на продажу должен быть выбран клиент.");
				return;
			}

			var nomenclaturesJournalViewModel = OpenNomenclaturesJournalViewModel();
			nomenclaturesJournalViewModel.OnSelectResult += NomenclatureToClient;
		}

		void NomenclatureToClient(object sender, JournalSelectedEventArgs e)
		{
			var selectedNode = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();
			if(selectedNode == null) {
				return;
			}
			AddNomenclatureToClient(UoW.Session.Get<Nomenclature>(selectedNode.Id));
		}

		void AddNomenclatureToClient(Nomenclature nomenclature)
		{
			Order.AddEquipmentNomenclatureToClient(nomenclature, UoW);
		}

		protected void OnButtonAddEquipmentFromClientClicked(object sender, EventArgs e)
		{
			if(Order.Client == null)
			{
				MessageDialogHelper.RunWarningDialog("Для добавления товара на продажу должен быть выбран клиент.");
				return;
			}
			
			var nomenclaturesJournalViewModel = OpenNomenclaturesJournalViewModel();
			nomenclaturesJournalViewModel.OnSelectResult += NomenclatureFromClient;
		}

		private NomenclaturesJournalViewModel OpenNomenclaturesJournalViewModel()
		{
			return Startup.MainWin.NavigationManager.OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
				MyTab,
				f =>
				{
					f.AvailableCategories = Nomenclature.GetCategoriesForGoods();
					f.SelectCategory = NomenclatureCategory.equipment;
					f.SelectSaleCategory = SaleCategory.notForSale;
					f.CanChangeOnlyOnlineNomenclatures = false;
				},
				OpenPageOptions.AsSlave,
				vm =>
				{
					vm.CalculateQuantityOnStock = true;
					vm.SelectionMode = JournalSelectionMode.Single;
				})
			.ViewModel;
		}

		void NomenclatureFromClient(object sender, JournalSelectedEventArgs e)
		{
			var selectedNode = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();
			if(selectedNode == null) {
				return;
			}
			AddNomenclatureFromClient(UoW.Session.Get<Nomenclature>(selectedNode.Id));
		}

		void AddNomenclatureFromClient(Nomenclature nomenclature)
		{
			Order.AddEquipmentNomenclatureFromClient(nomenclature, UoW);
		}
	}
}
