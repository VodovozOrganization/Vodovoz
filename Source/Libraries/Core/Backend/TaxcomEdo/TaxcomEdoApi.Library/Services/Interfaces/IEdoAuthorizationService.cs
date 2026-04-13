using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Contracts.Authorization;

namespace TaxcomEdoApi.Library.Services.Interfaces;

public interface IEdoAuthorizationService
{
	Task<string> LoginAsync(
		string login,
		string password,
		HttpRequestType requestType = HttpRequestType.Post,
		CancellationToken cancellationToken = default);
	Task<string> CertificateLoginAsync(byte[] certificateData, CancellationToken cancellationToken = default);
}
