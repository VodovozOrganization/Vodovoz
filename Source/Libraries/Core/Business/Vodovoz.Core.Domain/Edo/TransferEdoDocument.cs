namespace Vodovoz.Core.Domain.Edo
{
	public class TransferEdoDocument : OutgoingEdoDocument
	{
		private int _transferTaskId;
		public virtual int TransferTaskId
		{
			get => _transferTaskId;
			set => SetField(ref _transferTaskId, value);
		}
	}
}
