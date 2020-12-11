using VodovozSalesReceiptsService.DTO;

namespace VodovozSalesReceiptsService
{
    public interface ISalesReceiptSender
    {
        /// <summary>
        /// Отправляет подготовленные документы на кассовые аппараты, указанные в <see cref="PreparedReceiptNode"/>.<see cref="PreparedReceiptNode.CashBox"/>
        /// </summary>
        /// <param name="preparedReceiptNodes">
        /// Массив валидных документов, чеков и кассовых апартов
        /// </param>
        /// <param name="timeoutInSeconds">
        /// Таймаут операции в секундах
        /// </param>
        /// <returns>
        /// Массив с отправленными документами, чеками и результатами отправки,
        /// записанными в <see cref="PreparedReceiptNode"/>.<see cref="PreparedReceiptNode.SendResultCode"/>
        /// </returns>
        PreparedReceiptNode[] SendReceipts(PreparedReceiptNode[] preparedReceiptNodes, uint timeoutInSeconds = 300);
    }
}