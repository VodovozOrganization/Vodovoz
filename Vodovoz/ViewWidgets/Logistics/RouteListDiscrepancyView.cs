using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository.Logistics;
using QS.DomainModel.UoW;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListDiscrepancyView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		public RouteListDiscrepancyView()
		{
			this.Build();
			Configure();
			Items = new List<Discrepancy>();
		}

		List<Discrepancy> items;
		public List<Discrepancy> Items {
			get => items;
			set {
				items = value;
				ytreeview2.ItemsDataSource = items;
			}
		}

		public IList<RouteListControlNotLoadedNode> ItemsLoaded { get; set; }

		protected void Configure()
		{
			var colorRed = new Gdk.Color(0xee, 0x66, 0x66);
			var colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
			ytreeview2.ColumnsConfig = ColumnsConfigFactory.Create<Discrepancy>()
				.AddColumn("Название")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Недопо-\nгрузка")
					.AddNumericRenderer(node => node.FromWarehouse)
				.AddColumn("Выгру-\nзка")
					.AddNumericRenderer(node => node.ToWarehouse)
				.AddColumn("Не-\nдовоз")
					.AddTextRenderer(node => node.Returns)
				.AddColumn("От \nклиента")
					.AddNumericRenderer(node => node.PickedUpFromClient)
				.AddColumn("Расхо-\nждения")
					.AddNumericRenderer(node => node.Remainder)
						.Adjustment(new Gtk.Adjustment(0, 0, 9999, 1, 1, 0))
				.AddColumn("Ущерб")
					.AddTextRenderer(x => x.SumOfDamage.ToString("C"))
				.AddColumn("Штраф")
					.AddToggleRenderer(x => x.UseFine).ToggledEvent(UseFine_Toggled)
				.RowCells()
					.AddSetter<CellRenderer>((cell, node) => cell.CellBackgroundGdk = node.Remainder == 0 ? colorWhite : colorRed)
				.Finish();
		}

		public event EventHandler FineChanged;

		void UseFine_Toggled(object o, ToggledArgs args)
		{
			//Вызываем через Application.Invoke что бы событие вызывалось уже после того как поле обновилось.
			Application.Invoke(delegate {
				FineChanged?.Invoke(this, EventArgs.Empty);
			});
		}

		public void FindDiscrepancies(IList<RouteListItem> items, List<RouteListRepository.ReturnsNode> allReturnsToWarehouse)
		{
			Items = GetDiscrepancies(items, allReturnsToWarehouse);
		}

		List<Discrepancy> GetDiscrepancies(IList<RouteListItem> items, List<RouteListRepository.ReturnsNode> allReturnsToWarehouse)
		{
			List<Discrepancy> result = new List<Discrepancy>();

			//ТОВАРЫ
			var orderClosingItems = items.Where(item => item.TransferedTo == null || item.TransferedTo.NeedToReload)
										 .SelectMany(item => item.Order.OrderItems)
										 .Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
										 .Where(item => item.Nomenclature.Category != NomenclatureCategory.bottle)
										 .ToList();

			foreach(var orderItem in orderClosingItems) {
				var discrepancy = new Discrepancy {
					Nomenclature = orderItem.Nomenclature,
					ClientRejected = orderItem.ReturnedCount,
					Name = orderItem.Nomenclature.Name
				};
				AddDiscrepancy(result, discrepancy);
			}

			//ОБОРУДОВАНИЕ
			var orderEquipments = items.Where(item => item.TransferedTo == null || item.TransferedTo.NeedToReload)
									   .SelectMany(item => item.Order.OrderEquipments)
									   .Where(item => Nomenclature.GetCategoriesForShipment().Contains(item.Nomenclature.Category))
									   .ToList();
			foreach(var orderEquip in orderEquipments) {
				var discrepancy = new Discrepancy {
					Nomenclature = orderEquip.Nomenclature,
					Name = orderEquip.Nomenclature.Name
				};

				if(orderEquip.Direction == Domain.Orders.Direction.Deliver)
					discrepancy.ClientRejected = orderEquip.ReturnedCount;
				else
					discrepancy.PickedUpFromClient = orderEquip.ActualCount ?? 0;

				AddDiscrepancy(result, discrepancy);
			}

			//ДОСТАВЛЕНО НА СКЛАД
			var warehouseItems = allReturnsToWarehouse.Where(x => x.NomenclatureCategory != NomenclatureCategory.bottle)
													  .ToList();
			foreach(var whItem in warehouseItems) {
				var discrepancy = new Discrepancy {
					Nomenclature = whItem.Nomenclature,
					ToWarehouse = whItem.Amount,
					Name = whItem.Name
				};
				AddDiscrepancy(result, discrepancy);
			}

			if(ItemsLoaded != null && ItemsLoaded.Any()) {
				var loadedItems = ItemsLoaded.Where(x => x.Nomenclature.Category != NomenclatureCategory.bottle);
				foreach(var item in loadedItems) {
					var discrepancy = new Discrepancy {
						Nomenclature = item.Nomenclature,
						FromWarehouse = item.CountNotLoaded,
						Name = item.Nomenclature.Name
					};

					AddDiscrepancy(result, discrepancy);
				}
			}

			return result;
		}

		/// <summary>
		/// Добавляет новое расхождение если такой номенклатуры нет в списке, 
		/// иначе прибавляет все значения к найденной в списке номенклатуре
		/// </summary>
		void AddDiscrepancy(List<Discrepancy> list, Discrepancy item)
		{
			var existingDiscrepancy = list.FirstOrDefault(x => x.Nomenclature == item.Nomenclature);
			if(existingDiscrepancy == null) {
				list.Add(item);
			} else {
				existingDiscrepancy.ClientRejected += item.ClientRejected;
				existingDiscrepancy.PickedUpFromClient += item.PickedUpFromClient;
				existingDiscrepancy.ToWarehouse += item.ToWarehouse;
				existingDiscrepancy.FromWarehouse += item.FromWarehouse;
			}
		}
	}

	public class EquipmentTypeGroupingResult
	{
		public EquipmentType EquipmentType { get; set; }
		public int Amount { get; set; }
		public static EquipmentTypeGroupingResult Selector(EquipmentType type, IEnumerable<int> amounts)
		{
			return new EquipmentTypeGroupingResult {
				EquipmentType = type,
				Amount = amounts.Sum()
			};
		}
	}

	public class Discrepancy
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public Nomenclature Nomenclature { get; set; }

		/// <summary>
		/// Количество которое необходимо забрать у клиента
		/// </summary>
		/// <value>The picked up from client.</value>
		public decimal PickedUpFromClient { get; set; }

		/// <summary>
		/// Недовезенное количество
		/// </summary>
		/// <value>The client rejected.</value>
		public decimal ClientRejected { get; set; }

		/// <summary>
		/// Выгружено на склад
		/// </summary>
		/// <value>To warehouse.</value>
		public decimal ToWarehouse { get; set; }

		/// <summary>
		/// Погружено на складе
		/// </summary>
		public decimal FromWarehouse { get; set; }

		public bool Trackable { get; set; }
		public bool UseFine { get; set; }

		/// <summary>
		/// Остаток
		/// </summary>
		public decimal Remainder => FromWarehouse + ToWarehouse - ClientRejected - PickedUpFromClient;

		/// <summary>
		/// Недовоз
		/// </summary>
		public string Returns => string.Format("{0}", ClientRejected);

		/// <summary>
		/// Серийный номер
		/// </summary>
		public string Serial {
			get {
				if(Trackable)
					return Id > 0 ? Id.ToString() : "(не определен)";
				return string.Empty;
			}
		}

		/// <summary>
		/// Ущерб
		/// </summary>
		public decimal SumOfDamage => Nomenclature == null ? 0 : Nomenclature.SumOfDamage * (-Remainder);
	}
}