using CloudPaymentsApi.Library.Models;
using CloudPaymentsApi.Library.Requests;
using CloudPaymentsApi.Library.Responses;
using System.Threading;
using System.Threading.Tasks;

namespace CloudPaymentsApi.Client
{
	public interface ICloudPaymentsApiClient
	{
		Task<CloudPaymentsResponse<CloudPaymentsTransaction>> GetTransactionAsync(long transactionId, CancellationToken cancellationToken);
		Task<CloudPaymentsResponse<CloudPaymentsRefundResult>> RefundAsync(CloudPaymentsRefundRequest request, string idempotenceKey, CancellationToken cancellationToken);
	}
}
