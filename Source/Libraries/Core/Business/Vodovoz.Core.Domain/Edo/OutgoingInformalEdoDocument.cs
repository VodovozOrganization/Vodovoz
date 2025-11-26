namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Исходящий неформализованный ЭДО документ 
	/// </summary>
	public class OutgoingInformalEdoDocument : OutgoingEdoDocument
	{
		private int _informalDocumentTaskId;
		public virtual int InformalDocumentTaskId
		{
			get => _informalDocumentTaskId;
			set => SetField(ref _informalDocumentTaskId, value);
		}
	}
}

