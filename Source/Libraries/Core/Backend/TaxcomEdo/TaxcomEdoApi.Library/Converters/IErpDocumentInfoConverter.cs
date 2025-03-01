using Edo.Contracts.Messages.Dto;
using Taxcom.Client.Api.Document.DocumentByFormat1115131;

namespace TaxcomEdoApi.Library.Converters
{
	public interface IErpDocumentInfoConverter
	{
		/// <summary>
		/// Получение информации о покупателе для документа
		/// </summary>
		/// <param name="customer">Информация о покупателе из Erp</param>
		/// <returns></returns>
		UchastnikTip ConvertCounterpartyToCustomerInfo(CustomerInfo customer);
		/// <summary>
		/// Получение информации о грузополучателе
		/// </summary>
		/// <param name="consignee">Информация о грузополучателе из Erp</param>
		/// <returns></returns>
		UchastnikTip ConvertCounterpartyToConsigneeInfo(ConsigneeInfo consignee);
		/// <summary>
		/// Получение информации о продавце
		/// </summary>
		/// <param name="org">Информация об организации продавца</param>
		/// <returns></returns>
		UchastnikTip ConvertOrganizationToSellerInfo(OrganizationInfo org);
	}
}
