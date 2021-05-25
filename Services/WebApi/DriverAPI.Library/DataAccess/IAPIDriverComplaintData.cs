using DriverAPI.Library.Models;
using System.Collections.Generic;

namespace DriverAPI.Library.DataAccess
{
	public interface IAPIDriverComplaintData
	{
		IEnumerable<APIDriverComplaintReason> GetPinnedComplaintReasons();
	}
}