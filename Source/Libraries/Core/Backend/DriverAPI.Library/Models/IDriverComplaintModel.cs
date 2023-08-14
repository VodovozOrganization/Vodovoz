using DriverAPI.Library.DTOs;
using System.Collections.Generic;

namespace DriverAPI.Library.Models
{
	public interface IDriverComplaintModel
	{
		IEnumerable<DriverComplaintReasonDto> GetPinnedComplaintReasons();
	}
}
