using DriverAPI.Library.DTOs;
using Vodovoz.Domain.Complaints;

namespace DriverAPI.Library.Converters
{
	public class DriverComplaintReasonConverter
	{
		public DriverComplaintReasonDto convertToAPIDriverComplaintReason(DriverComplaintReason driverComplaint)
		{
			return new DriverComplaintReasonDto()
			{
				Id = driverComplaint.Id,
				Name = driverComplaint.Name
			};
		}
	}
}
