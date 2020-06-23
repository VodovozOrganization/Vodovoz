using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using VodovozMobileService.DTO;

namespace VodovozMobileService
{
	[ServiceContract]
	public interface IMobileService
	{
		[OperationContract]
		[
			WebGet(
				UriTemplate = "/Catalog/{type}/",
				ResponseFormat = WebMessageFormat.Json
			)
		]
		List<NomenclatureDTO> GetGoods(CatalogType type);

		[OperationContract]
		[
			WebGet(
				UriTemplate = "/Catalog/Images/{filename}",
				ResponseFormat = WebMessageFormat.Json
			)
		]
		Stream GetImage(string filename);

		[OperationContract]
		[
			WebInvoke(
				UriTemplate = "/Orders/New",
				Method = "POST",
				RequestFormat = WebMessageFormat.Json,
				ResponseFormat = WebMessageFormat.Json
			)
		]
		CreateOrderResponseDTO Order(MobileOrderDTO ord);
	}
}