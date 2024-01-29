namespace Vodovoz.ViewModels.Counterparties
{
	public partial class ClientEquipmentBalanceJournalViewModel
	{
		public class ClientEquipmentBalanceNode
		{
			public int Id { get; set; }

			public string SerialNumber => SerialNumberInt.ToString();

			public int SerialNumberInt { get; set; }

			public bool IsOur { get; set; }

			public string Client { get; set; }

			public string Address { get; set; }

			public string NomenclatureName { get; set; }
		}
	}
}
