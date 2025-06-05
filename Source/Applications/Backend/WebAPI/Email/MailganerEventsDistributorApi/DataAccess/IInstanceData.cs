using MailganerEventsDistributorApi.DTO;

namespace MailganerEventsDistributorApi.DataAccess
{
	public interface IInstanceData
	{
		InstanceDto GetInstanceByDatabaseId(int Id);
	}
}