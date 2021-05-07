using DriverAPI.Library.Converters;
using DriverAPI.Library.Models;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Collections.Generic;
using Vodovoz.EntityRepositories.Complaints;

namespace DriverAPI.Library.DataAccess
{
    public class APIDriverComplaintData : IAPIDriverComplaintData
    {
        private readonly ILogger<APIDriverComplaintData> logger;
        private readonly IComplaintsRepository complaintsRepository;
        private readonly DriverComplaintReasonConverter driverComplaintReasonConverter;
        private readonly IUnitOfWork unitOfWork;

        public APIDriverComplaintData(
            ILogger<APIDriverComplaintData> logger,
            IComplaintsRepository complaintsRepository,
            DriverComplaintReasonConverter driverComplaintReasonConverter,
            IUnitOfWork unitOfWork)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));
            this.driverComplaintReasonConverter = driverComplaintReasonConverter ?? throw new ArgumentNullException(nameof(driverComplaintReasonConverter));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public IEnumerable<APIDriverComplaintReason> GetPinnedComplaintReasons()
        {
            return complaintsRepository.GetDriverComplaintPopularReasons(unitOfWork)
                .Select(x => driverComplaintReasonConverter.convertToAPIDriverComplaintReason(x));
        }
    }
}
