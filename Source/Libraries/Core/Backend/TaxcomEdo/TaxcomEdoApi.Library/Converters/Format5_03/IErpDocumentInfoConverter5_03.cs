using Edo.Contracts.Messages.Dto;
using Edo.Contracts.Xml.Documents.FormalizedDocuments.UPD;

namespace TaxcomEdoApi.Library.Converters.Format5_03
{
	public interface IErpDocumentInfoConverter5_03
	{
		/// <summary>
		/// Получение информации о покупателе для документа
		/// </summary>
		/// <param name="customer">Информация о покупателе из Erp</param>
		/// <returns></returns>
		УчастникТип ConvertCounterpartyToCustomerInfo(CustomerInfo customer);
		/// <summary>
		/// Получение информации о грузополучателе
		/// </summary>
		/// <param name="consignee">Информация о грузополучателе из Erp</param>
		/// <returns></returns>
		УчастникТип ConvertCounterpartyToConsigneeInfo(ConsigneeInfo consignee);
		/// <summary>
		/// Получение информации о продавце
		/// </summary>
		/// <param name="org">Информация об организации продавца</param>
		/// <returns></returns>
		УчастникТип ConvertOrganizationToSellerInfo(OrganizationInfo org);
	}
}
