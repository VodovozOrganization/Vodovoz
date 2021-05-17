using DriverAPI.Library.Models;
using Vodovoz.Domain.Complaints;

namespace DriverAPI.Library.Converters
{
    public class DriverComplaintReasonConverter
    {
        public APIDriverComplaintReason convertToAPIDriverComplaintReason(DriverComplaintReason driverComplaint)
        {
            return new APIDriverComplaintReason()
            {
                Id = driverComplaint.Id,
                Name = driverComplaint.Name
            };
        }
    }
}
