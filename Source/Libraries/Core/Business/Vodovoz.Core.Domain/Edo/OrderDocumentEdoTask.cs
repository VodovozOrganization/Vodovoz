using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class OrderDocumentEdoTask : EdoTask
	{
		private InformalEdoRequest _informalEdoRequest;
		public override EdoTaskType TaskType => EdoTaskType.InformalOrderDocument;

		/// <summary>
		/// Заявка ЭДО отправки документа заказа
		/// </summary>
		[Display(Name = "Заявка ЭДО отправки документа заказа")]
		public virtual InformalEdoRequest InformalEdoRequest
		{
			get => _informalEdoRequest;
			set => SetField(ref _informalEdoRequest, value);
		}
	}
}
