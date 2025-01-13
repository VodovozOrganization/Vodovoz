using DriverApi.Contracts.V6;
using System.Collections.Generic;
using Vodovoz.Errors;

namespace DriverAPI.Library.V6.Services
{
	/// <summary>
	/// 
	/// </summary>
	public interface IDriverComplaintService
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		Result<IEnumerable<DriverComplaintReasonDto>> GetPinnedComplaintReasons();
	}
}
