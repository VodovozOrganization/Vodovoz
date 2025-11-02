namespace Vodovoz.ViewModels.Organizations
{
	public class AccountJournalNode
	{
		public int Id {  get; set; }
		public bool IsDefault { get; set; }
		public string Alias { get; set; }
		public string BankName { get; set; }
		public string AccountNumber { get; set; }
		public string Title => AccountNumber;
	}
}
