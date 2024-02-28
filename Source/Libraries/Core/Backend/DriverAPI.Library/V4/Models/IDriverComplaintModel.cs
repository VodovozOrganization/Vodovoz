using DriverApi.Contracts.V4;
using System.Collections.Generic;

namespace DriverAPI.Library.V4.Models
{
	/// <summary>
	/// 
	/// </summary>
	public interface IDriverComplaintModel
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		IEnumerable<DriverComplaintReasonDto> GetPinnedComplaintReasons();
	}
}
