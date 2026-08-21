using TaxcomEdo.Client;
using TaxcomEdo.Client.Configs;

namespace Taxcom.Docflow.Utility
{
	public interface ITaxcomApiFactory
	{
		ITaxcomApiClient Create(TaxcomApiOptions taxcomApiOptions);

		/// <summary>
		/// Создать API клиент по организации
		/// </summary>
		/// <param name="organizationId">Идентификатор организации</param>
		/// <returns>API клиент</returns>
		ITaxcomApiClient Create(int organizationId, string edoAccount);
	}
}
