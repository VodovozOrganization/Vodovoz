using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Задача ЭДО по ручной заявке
	/// </summary>
	public class ManualEdoTask : EdoTask
	{
		private FormalEdoRequest _formalEdoRequest;
		public override EdoTaskType TaskType => EdoTaskType.Document;

		/// <summary>
		/// Заявка ЭДО отправки формализованного документа
		/// </summary>
		[Display(Name = "Заявка ЭДО отправки формализованного документа")]
		public virtual FormalEdoRequest FormalEdoRequest
		{
			get => _formalEdoRequest;
			set => SetField(ref _formalEdoRequest, value);
		}
	}
}
