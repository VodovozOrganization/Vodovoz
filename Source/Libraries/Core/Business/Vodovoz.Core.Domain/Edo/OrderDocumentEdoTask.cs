using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	public abstract class OrderDocumentEdoTask : EdoTask
	{
		/// <summary>
		/// Заменить на EdoTaskType.InformalDocument
		/// </summary>
		public override EdoTaskType TaskType => EdoTaskType.EquipmentTransfer;

		/// <summary>
		/// Тип документа заказа
		/// </summary>
		[Display(Name = "Тип документа заказа")]
		public abstract OrderDocumentType? DocumentType { get; }
	}
}
