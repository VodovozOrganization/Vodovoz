using MailjetEventMessagesDistributorAPI.DTO;

namespace MailjetEventMessagesDistributorAPI.DataAccess
{
	public interface IInstanceData
	{
		InstanceDto GetInstanceByDatabaseId(int Id);
	}
}