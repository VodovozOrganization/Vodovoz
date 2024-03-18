using System.Collections.Generic;

namespace Pacs.Server.Phones
{
	public interface IPhoneRepository
	{
		IEnumerable<PhoneAssignment> GetPhoneAssignments();
	}
}
