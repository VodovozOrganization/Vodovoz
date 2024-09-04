using System.Threading.Tasks;
using TaxcomEdo.Contracts;

namespace EdoDocumentsConsumer.Services
{
	public interface ITaxcomService
	{
		Task SendDataForCreateUpdByEdo(InfoForCreatingEdoUpd updData);
	}
}
