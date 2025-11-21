namespace TaxcomEdo.Client.Configs
{
	/// <summary>
	/// Настройки клиента Taxcom Api
	/// </summary>
	public class TaxcomApiOptions
	{
		/// <summary>
		/// Секция с настройками
		/// </summary>
		public const string Path = "TaxcomApiOptions";
		/// <summary>
		/// Основной адрес
		/// </summary>
		public string MainEdoAccountBaseAddress { get; set; }
		/// <summary>
		/// Эндпойнт отправки УПД по объемно-сортовому учету(контейнеров)
		/// </summary>
		public string SendBulkAccountingUpdEndpoint { get; set; }
		/// <summary>
		/// Эндпойнт отправки новых УПД(поэкземплярный учет)
		/// </summary>
		public string SendIndividualAccountingUpdEndpoint { get; set; }
		/// <summary>
		/// Эндпойнт отправки акта приёма-передачи оборудования
		/// </summary>
		public string SendEquipmentTransferEndpoint { get; set; }
		/// <summary>
		/// Эндпойнт отправки счета
		/// </summary>
		public string SendBillEndpoint { get; set; }
		/// <summary>
		/// Эндпойнт отправки счета без отгрузки на долг
		/// </summary>
		public string SendBillWithoutShipmentForDebtEndpoint { get; set; }
		/// <summary>
		/// Эндпойнт отправки счета без отгрузки на постоплату
		/// </summary>
		public string SendBillWithoutShipmentForPaymentEndpoint { get; set; }
		/// <summary>
		/// Эндпойнт отправки счета без отгрузки на предоплату
		/// </summary>
		public string SendBillWithoutShipmentForAdvancePaymentEndpoint { get; set; }
		/// <summary>
		/// Эндпойнт получения списка обновленных контактов
		/// </summary>
		public string GetContactListUpdatesEndPoint { get; set; }
		/// <summary>
		/// Эндпойнт принятия входящего приглашения к ЭДО
		/// </summary>
		public string AcceptContactEndPoint { get; set; }
		/// <summary>
		/// Эндпойнт получения всех документов документооборота
		/// </summary>
		public string GetDocFlowRawDataEndPoint { get; set; }
		/// <summary>
		/// Эндпойнт плучениявсех изменений документооборотов
		/// </summary>
		public string GetDocFlowsUpdatesEndPoint { get; set; }
		/// <summary>
		/// Эндпойнт запуска всех необходимых транзакций
		/// </summary>
		public string AutoSendReceiveEndpoint { get; set; }
		/// <summary>
		/// Эндпойнт аннулирования документооборота
		/// </summary>
		public string OfferCancellationEndpoint { get; set; }
		/// <summary>
		/// Эндпойнт подписания входящего документооборота
		/// </summary>
		public string AcceptIngoingDocflowEndpoint { get; set; }
		/// <summary>
		/// Эндпойнт получения статуса документооборота
		/// </summary>
		public string GetDocflowStatusEndpoint { get; set; }

		/// <summary>
		/// Основные адреса наших организаций
		/// </summary>
		public EdoAccountBaseAddress[] EdoAccountBaseAddresses { get; set; }
	}

	/// <summary>
	/// Основной адрес нашей организации
	/// </summary>
	public class EdoAccountBaseAddress
	{
		/// <summary>
		/// 
		/// </summary>
		public string EdoAccountId { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public string BaseAddress { get; set; }
	}
}
