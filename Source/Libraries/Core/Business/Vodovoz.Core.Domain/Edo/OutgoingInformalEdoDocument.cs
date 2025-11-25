namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Исходящий неформализованный ЭДО документ 
	/// </summary>
	public class OutgoingInformalEdoDocument : OutgoingEdoDocument
	{
		private int _documentTaskId;
		public virtual int DocumentTaskId
		{
			get => _documentTaskId;
			set => SetField(ref _documentTaskId, value);
		}
	}
}

