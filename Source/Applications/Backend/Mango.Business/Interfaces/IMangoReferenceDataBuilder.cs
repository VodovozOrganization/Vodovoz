using Mango.Business.Models;
using Mango.Contracts.V1.Response;

namespace Mango.Business.Interfaces
{
	public interface IMangoReferenceDataBuilder
	{
		MangoReferenceData Build(GroupsResponse response);
	}
}
