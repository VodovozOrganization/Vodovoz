namespace Vodovoz.Core.Domain.Edo
{
	// Переименовать
	/// <summary>
	/// Исходящий документ ЭДО акта приёма-передачи оборудования
	/// </summary>
	public class EquipmentTransferEdoDocument : OutgoingEdoDocument
	{
		private int _documentTaskId;
		public virtual int DocumentTaskId
		{
			get => _documentTaskId;
			set => SetField(ref _documentTaskId, value);
		}
	}
}

