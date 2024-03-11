using DriverApi.Contracts.V5;
using System.Collections.Generic;
using Vodovoz.Errors;

namespace DriverAPI.Library.V5.Services
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
