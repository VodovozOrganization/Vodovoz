using VodovozInfrastructure.Cryptography;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Колонки журнала платежей
	/// </summary>
	public class PaymentsJournalColumns
	{
		public static string Id => "Код";
		public static string Number => "№";
		public static string Date => "Дата";
		public static string Total => "Cумма";
		public static string Orders => "Заказы";
		public static string Payer => "Плательщик";
		public static string Counterparty => "Контрагент";
		public static string Organization => "Получатель\n/Организация списания";
		public static string OrganizationBank => "Банк";
		public static string OrganizationAccount => "Р/сч";
		public static string Purpose => "Назначение платежа\n/Причина списания";
		public static string ProfitCategory => "Категория дохода/расхода\n/Статья расхода";
		public static string IsManuallyCreated => "Создан вручную";
		public static string UnAllocatedSum => "Нераспределенная сумма";
		public static string DocumentType => "Тип документа";
	}
}
