using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Документ самовывоза
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "документы самовывоза",
		Nominative = "документ самовывоза")]
	[HistoryTrace]
	public class SelfDeliveryDocumentEntity : Document, IDomainObject
	{
		private Warehouse _warehouse;
		private OrderEntity _order;
		private string _comment;
		private IObservableList<SelfDeliveryDocumentItem> _items = new ObservableList<SelfDeliveryDocumentItem>();
		private IList<SelfDeliveryDocumentReturned> _returnedItems = new List<SelfDeliveryDocumentReturned>();

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override DateTime TimeStamp
		{
			get => base.TimeStamp;
			set
			{
				base.TimeStamp = value;

				if(!NHibernate.NHibernateUtil.IsInitialized(Items))
				{
					return;
				}

				foreach(var item in Items)
				{
					if(item.GoodsAccountingOperation != null
						&& item.GoodsAccountingOperation.OperationTime != TimeStamp)
					{
						item.GoodsAccountingOperation.OperationTime = TimeStamp;
					}
				}
			}
		}

		/// <summary>
		/// Заказ, к которому относится документ самовывоза
		/// </summary>
		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		/// <summary>
		/// Склад, на который оформляется самовывоз
		/// </summary>
		[Required(ErrorMessage = "Склад должен быть указан.")]
		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set => SetField(ref _warehouse, value);
		}

		/// <summary>
		/// Комментарий к самовывозу
		/// </summary>
		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		/// <summary>
		/// Строки самовывоза
		/// </summary>
		[Display(Name = "Строки самовывоза")]
		public virtual IObservableList<SelfDeliveryDocumentItem> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}

		/// <summary>
		/// Строки возврата
		/// </summary>
		[Display(Name = "Строки возврата")]
		public virtual IList<SelfDeliveryDocumentReturned> ReturnedItems
		{
			get => _returnedItems;
			set => SetField(ref _returnedItems, value);
		}

		#region Не сохраняемые

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public virtual string Title => $"Самовывоз №{Id} от {TimeStamp:d}";

		#endregion

		/// <summary>
		/// Заполнение строк самовывоза по заказу
		/// </summary>
		public virtual void FillByOrder()
		{
			Items.Clear();
			if(Order == null)
			{
				return;
			}

			foreach(var orderItem in Order.OrderItems)
			{
				if(!NomenclatureEntity
					.GetCategoriesForShipment()
					.Contains(orderItem.Nomenclature.Category))
				{
					continue;
				}

				if(!Items.Any(i => i.Nomenclature == orderItem.Nomenclature))
				{
					Items.Add(
						new SelfDeliveryDocumentItem
						{
							Document = this,
							Nomenclature = orderItem.Nomenclature,
							OrderItem = orderItem,
							OrderEquipment = null,
							Amount = GetNomenclaturesCountInOrder(orderItem.Nomenclature.Id)
						});
				}

			}

			foreach(var orderEquipment in Order.OrderEquipments
				.Where(x => x.Direction == Direction.Deliver))
			{
				if(!Items.Any(i => i.Nomenclature == orderEquipment.Nomenclature))
				{
					Items.Add(
						new SelfDeliveryDocumentItem
						{
							Document = this,
							Nomenclature = orderEquipment.Nomenclature,
							OrderItem = null,
							OrderEquipment = orderEquipment,
							Amount = GetNomenclaturesCountInOrder(orderEquipment.Nomenclature.Id)
						});
				}
			}

			if(!ReturnedItems.Any(x => x.Id != 0))
			{
				ReturnedItems = Order.OrderEquipments
					.Where(x => x.Direction == Direction.PickUp)
					.GroupBy(x => (x.Nomenclature, x.DirectionReason, x.OwnType))
					.ToDictionary(x => x.Key, x => x.ToList())
					.Select(x => new SelfDeliveryDocumentReturned
					{
						Document = this,
						Nomenclature = x.Key.Nomenclature,
						ActualCount = 0,
						Amount = x.Value.Sum(e => e.Count),
						Direction = Direction.PickUp,
						DirectionReason = x.Key.DirectionReason,
						OwnType = x.Key.OwnType
					})
					.ToList();
			}
		}

		/// <summary>
		/// Получение количества номенклатуры в заказе
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual decimal GetNomenclaturesCountInOrder(int nomenclatureId)
		{
			decimal count = Order.OrderItems
				.Where(i => i.Nomenclature.Id == nomenclatureId)
				.Sum(i => i.Count);

			count += Order.OrderEquipments
				.Where(e => e.Nomenclature.Id == nomenclatureId
					&& e.Direction == Direction.Deliver)
				.Sum(e => e.Count);

			return count;
		}
	}
}
