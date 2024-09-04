using System;
using System.Net.Http;
using System.Threading.Tasks;
using EdoDocumentsConsumer.Configs;
using Microsoft.Extensions.Options;
using TaxcomEdo.Contracts;

namespace EdoDocumentsConsumer.Services
{
	public class TaxcomService : ITaxcomService
	{
		private readonly HttpClient _httpClient;
		private readonly TaxcomApiOptions _taxcomApiOptions;

		public TaxcomService(HttpClient client, IOptions<TaxcomApiOptions> taxcomApiOptions)
		{
			_httpClient = client ?? throw new ArgumentNullException(nameof(client));
			_taxcomApiOptions = (taxcomApiOptions ?? throw new ArgumentNullException(nameof(taxcomApiOptions))).Value;
		}
		
		public async Task SendDataForCreateUpdByEdo(InfoForCreatingEdoUpd data)
		{
			await SendData(_taxcomApiOptions.SendUpdEndpoint, data);
		}
		
		public async Task SendDataForCreateBillByEdo(InfoForCreatingEdoBill data)
		{
			await SendData(_taxcomApiOptions.SendBillEndpoint, data);
		}
		
		public async Task SendDataForCreateBillWithoutShipmentByEdo(InfoForCreatingBillWithoutShipmentEdo data)
		{
			await SendData(_taxcomApiOptions.SendBillsWithoutShipmentEndpoint, data);
		}

		private async Task SendData<T>(string endPoint, T data)
		{
			await _httpClient.PostAsJsonAsync(endPoint, data);
		}
	}
}
