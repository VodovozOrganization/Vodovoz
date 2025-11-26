using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Задача ЭДО документа заказа
	/// </summary>
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
