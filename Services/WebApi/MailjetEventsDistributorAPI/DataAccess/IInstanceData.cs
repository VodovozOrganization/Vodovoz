using MailjetEventsDistributorAPI.DTO;

namespace MailjetEventsDistributorAPI.DataAccess
{
	public interface IInstanceData
	{
		InstanceDto GetInstanceByDatabaseId(int Id);
	}
}