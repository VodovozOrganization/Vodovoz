using DriverAPI.Library.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriverAPI.Library.DataAccess
{
    public class APIDriverComplaintData : IAPIDriverComplaintData
    {
        private readonly ILogger<APIDriverComplaintData> logger;

        public APIDriverComplaintData(ILogger<APIDriverComplaintData> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<APIDriverComplaintReason> GetPinnedComplaintReasons()
        {
            return new List<APIDriverComplaintReason>();
        }
    }
}
