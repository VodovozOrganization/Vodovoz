using TaxcomEdo.Client;
using TaxcomEdo.Client.Configs;

namespace Taxcom.Docflow.Utility
{
	public interface ITaxcomApiFactory
	{
		ITaxcomApiClientSdkVersion Create(TaxcomApiOptions taxcomApiOptions);
	}
}
