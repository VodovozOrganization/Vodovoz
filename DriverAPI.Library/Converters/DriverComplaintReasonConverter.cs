using DriverAPI.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
