using System.Threading.Tasks;
using Vodovoz.Core.Data.Documents;

namespace EdoDocumentsConsumer
{
	public interface ITaxcomService
	{
		Task SendDataForCreateUpdByEdo(InfoForCreatingEdoUpd updData);
	}
}
