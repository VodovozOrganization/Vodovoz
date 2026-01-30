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
		private IObservableList<SelfDeliveryDocumentItemEntity> _items = new ObservableList<SelfDeliveryDocumentItemEntity>();
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
			protected set => SetField(ref _order, value);
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
		public virtual IObservableList<SelfDeliveryDocumentItemEntity> Items
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
	}
}
