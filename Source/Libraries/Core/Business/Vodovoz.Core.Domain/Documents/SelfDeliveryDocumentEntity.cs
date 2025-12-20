using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Employees;
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
		private IObservableList<SelfDeliveryDocumentItemEntity> _items = new ObservableList<SelfDeliveryDocumentItemEntity>();

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
		/// Строки самовывоза
		/// </summary>
		[Display(Name = "Строки самовывоза")]
		public virtual IObservableList<SelfDeliveryDocumentItemEntity> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}
	}
}
