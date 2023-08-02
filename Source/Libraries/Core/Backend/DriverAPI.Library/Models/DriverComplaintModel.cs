using DriverAPI.Library.Converters;
using DriverAPI.Library.DTOs;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories.Complaints;

namespace DriverAPI.Library.Models
{
	internal class DriverComplaintModel : IDriverComplaintModel
	{
		private readonly IComplaintsRepository _complaintsRepository;
		private readonly DriverComplaintReasonConverter _driverComplaintReasonConverter;
		private readonly IUnitOfWork _unitOfWork;

		public DriverComplaintModel(
			IComplaintsRepository complaintsRepository,
			DriverComplaintReasonConverter driverComplaintReasonConverter,
			IUnitOfWork unitOfWork)
		{
			_complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));
			_driverComplaintReasonConverter = driverComplaintReasonConverter ?? throw new ArgumentNullException(nameof(driverComplaintReasonConverter));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		/// <summary>
		/// Получение популярных причин
		/// </summary>
		/// <returns>Перечисление популярных причин</returns>
		public IEnumerable<DriverComplaintReasonDto> GetPinnedComplaintReasons()
		{
			return _complaintsRepository.GetDriverComplaintPopularReasons(_unitOfWork)
				.Select(x => _driverComplaintReasonConverter.ConvertToAPIDriverComplaintReason(x));
		}
	}
}
