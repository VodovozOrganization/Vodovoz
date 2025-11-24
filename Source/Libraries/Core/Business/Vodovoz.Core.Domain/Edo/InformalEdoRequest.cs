using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Edo
{
	public class InformalEdoRequest : EdoRequest
	{
		private OrderEntity _order;
		private EdoDocumentType _documentType;

		/// <summary>
		/// Код заказа c актом приёма-передачи оборудования
		/// </summary>
		[Display(Name = "Код заказа")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value);
		}

		/// <summary>
		/// Тип документа
		/// </summary>
		[Display(Name = "Тип документа")]
		public virtual EdoDocumentType DocumentType
		{
			get => _documentType;
			set => SetField(ref _documentType, value);
		}
	}
}
