using GeoCoderApi.Client.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace GeoCoderApi.Client
{
	public interface IGeoCoderApiClient
	{
		Task<AddressResponse> GetAddressByCoordinateAsync(decimal latitude, decimal longitude, CancellationToken cancellationToken = default);
		Task<GeographicPointResponse> GetCoordinateAtAddressAsync(string address, CancellationToken cancellationToken = default);
	}
}
