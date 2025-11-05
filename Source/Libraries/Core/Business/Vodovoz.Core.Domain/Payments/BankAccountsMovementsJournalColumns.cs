namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Колонки журнала движений по р/сч
	/// </summary>
	public class BankAccountsMovementsJournalColumns
	{
		public static string Id => "Код";
		public static string StartDate => "Дата начала";
		public static string EndDate => "Дата окончания";
		public static string Account => "Р/сч";
		public static string Bank => "Банк";
		public static string Organization => "Организация";
		public static string Empty => "";
		public static string AmountFromDocument => "Сумма из выгрузки";
		public static string AmountFromProgram => "Сумма из ДВ";
		public static string Discrepancy => "Расхождение";
		public static string DiscrepancyDescription => "Описание расхождения";
	}
}
