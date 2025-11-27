using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Задача ЭДО по ручной заявке
	/// </summary>
	public class ManualEdoTask : EdoTask
	{
		private InformalEdoRequest _formalEdoRequest;
		public override EdoTaskType TaskType => EdoTaskType.InformalOrderDocument;

		/// <summary>
		/// Заявка ЭДО отправки документа заказа
		/// </summary>
		[Display(Name = "Заявка ЭДО отправки документа заказа")]
		public virtual InformalEdoRequest InformalEdoRequest
		{
			get => _formalEdoRequest;
			set => SetField(ref _formalEdoRequest, value);
		}
	}
}
