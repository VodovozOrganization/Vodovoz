using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	public abstract class OrderDocumentEdoTask : EdoTask
	{
		public override EdoTaskType TaskType => EdoTaskType.InformalOrderDocument;

		/// <summary>
		/// Заказ
		/// </summary>
		public virtual OrderEntity Order { get; set; }
		/// <summary>
		/// Тип документа заказа
		/// </summary>
		[Display(Name = "Тип документа заказа")]
		public abstract OrderDocumentType DocumentType { get; }
	}
}
