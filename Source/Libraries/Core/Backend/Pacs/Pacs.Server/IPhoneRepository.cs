using System.Collections.Generic;

namespace Pacs.Server
{
	public interface IPhoneRepository
	{
		IEnumerable<PhoneAssignment> GetPhoneAssignments();
	}
}
