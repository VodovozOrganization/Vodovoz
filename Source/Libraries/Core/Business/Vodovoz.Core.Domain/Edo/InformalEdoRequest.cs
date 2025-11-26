using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Неформализованная заявка ЭДО
	/// </summary>
	public class InformalEdoRequest : EdoRequest
	{
		private OrderEntity _order;
		private OrderDocumentEdoTask _task;
		private OrderDocumentType _orderDocumentType;

		/// <summary>
		/// Код заказа c документом заказа
		/// </summary>
		[Display(Name = "Код заказа")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		/// <summary>
		/// Задача ЭДО
		/// </summary>
		[Display(Name = "Задача ЭДО")]
		public virtual new OrderDocumentEdoTask Task
		{
			get => _task;
			set => SetField(ref _task, value);
		}
		public override EdoDocumentType DocumentType => EdoDocumentType.InformalOrderDocument;

		/// <summary>
		/// Тип документа заказа
		/// </summary>
		[Display(Name = "Тип документа заказа")]
		public virtual OrderDocumentType OrderDocumentType
		{
			get => _orderDocumentType;
			protected set => SetField(ref _orderDocumentType, value);
		}
	}
}
