using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Исходящий неформализованный ЭДО документ 
	/// </summary>
	public class OutgoingInformalEdoDocument : OutgoingEdoDocument
	{
		private int _informalDocumentTaskId;

		/// <summary>
		/// Идентификатор задачи исходящего неформализованного документа
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int InformalDocumentTaskId
		{
			get => _informalDocumentTaskId;
			set => SetField(ref _informalDocumentTaskId, value);
		}
	}
}

