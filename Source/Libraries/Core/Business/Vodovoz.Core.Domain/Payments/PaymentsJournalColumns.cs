using VodovozInfrastructure.Cryptography;

namespace Vodovoz.Core.Domain.Payments
{
	public class PaymentsJournalColumns
	{
		public static string IdColumn => "Код";
		public static string NumberColumn => "№";
		public static string DateColumn => "Дата";
		public static string TotalColumn => "Cумма";
		public static string OrdersColumn => "Заказы";
		public static string PayerColumn => "Плательщик";
		public static string CounterpartyColumn => "Контрагент";
		public static string OrganizationColumn => "Получатель\n/Организация списания";
		public static string OrganizationBankColumn => "Банк";
		public static string OrganizationAccountColumn => "Р/сч";
		public static string PurposeColumn => "Назначение платежа\n/Причина списания";
		public static string ProfitCategoryColumn => "Категория дохода/расхода\n/Статья расхода";
		public static string IsManuallyCreatedColumn => "Создан вручную";
		public static string UnAllocatedSumColumn => "Нераспределенная сумма";
		public static string DocumentTypeColumn => "Тип документа";
	}
}
