using DriverAPI.Library.Converters;
using DriverAPI.Library.Models;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories.Complaints;

namespace DriverAPI.Library.DataAccess
{
	public class APIDriverComplaintData : IAPIDriverComplaintData
	{
		private readonly IComplaintsRepository complaintsRepository;
		private readonly DriverComplaintReasonConverter driverComplaintReasonConverter;
		private readonly IUnitOfWork unitOfWork;

		public APIDriverComplaintData(
			IComplaintsRepository complaintsRepository,
			DriverComplaintReasonConverter driverComplaintReasonConverter,
			IUnitOfWork unitOfWork)
		{
			this.complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));
			this.driverComplaintReasonConverter = driverComplaintReasonConverter ?? throw new ArgumentNullException(nameof(driverComplaintReasonConverter));
			this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		/// <summary>
		/// Получение популярных причин
		/// </summary>
		/// <returns>Перечисление популярных причин</returns>
		public IEnumerable<APIDriverComplaintReason> GetPinnedComplaintReasons()
		{
			return complaintsRepository.GetDriverComplaintPopularReasons(unitOfWork)
				.Select(x => driverComplaintReasonConverter.convertToAPIDriverComplaintReason(x));
		}
	}
}
