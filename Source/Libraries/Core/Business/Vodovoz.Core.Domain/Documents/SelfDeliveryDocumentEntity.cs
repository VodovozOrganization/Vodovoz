using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
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
		private string _comment;
		private IObservableList<SelfDeliveryDocumentItemEntity> _items = new ObservableList<SelfDeliveryDocumentItemEntity>();

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

		#region Не сохраняемые

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public virtual string Title => $"Самовывоз №{Id} от {TimeStamp:d}";

		#endregion
	}
}
