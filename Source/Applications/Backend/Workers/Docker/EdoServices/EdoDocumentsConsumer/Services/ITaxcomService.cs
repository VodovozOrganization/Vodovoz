using System.Threading.Tasks;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Services
{
	public interface ITaxcomService
	{
		/// <summary>
		/// Передача данных по УПД в TaxcomApi для его формирования и отправки по ЭДО в Такском
		/// </summary>
		/// <param name="data">Данные для формирования УПД по ЭДО</param>
		/// <returns></returns>
		Task SendDataForCreateUpdByEdo(InfoForCreatingEdoUpd data);
		/// <summary>
		/// Передача данных по Счету в TaxcomApi для его формирования и отправки по ЭДО в Такском
		/// </summary>
		/// <param name="data">Данные для формирования Счета по ЭДО</param>
		/// <returns></returns>
		Task SendDataForCreateBillByEdo(InfoForCreatingEdoBill data);
		/// <summary>
		/// Передача данных по Счету без отгрузки в TaxcomApi для его формирования и отправки по ЭДО в Такском
		/// </summary>
		/// <param name="data">Данные для формирования Счета без отгрузки по ЭДО</param>
		/// <returns></returns>
		Task SendDataForCreateBillWithoutShipmentByEdo(InfoForCreatingBillWithoutShipmentEdo data);
	}
}
