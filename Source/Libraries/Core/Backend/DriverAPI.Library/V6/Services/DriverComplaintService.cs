﻿using DriverApi.Contracts.V6;
using DriverAPI.Library.V6.Converters;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories.Complaints;
using Vodovoz.Errors;

namespace DriverAPI.Library.V6.Services
{
	internal class DriverComplaintService : IDriverComplaintService
	{
		private readonly ILogger<DriverComplaintService> _logger;
		private readonly IComplaintsRepository _complaintsRepository;
		private readonly DriverComplaintReasonConverter _driverComplaintReasonConverter;
		private readonly IUnitOfWork _unitOfWork;

		public DriverComplaintService(
			ILogger<DriverComplaintService> logger,
			IComplaintsRepository complaintsRepository,
			DriverComplaintReasonConverter driverComplaintReasonConverter,
			IUnitOfWork unitOfWork)
		{
			_logger = logger;
			_complaintsRepository = complaintsRepository ?? throw new ArgumentNullException(nameof(complaintsRepository));
			_driverComplaintReasonConverter = driverComplaintReasonConverter ?? throw new ArgumentNullException(nameof(driverComplaintReasonConverter));
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
		}

		public Result<IEnumerable<DriverComplaintReasonDto>> GetPinnedComplaintReasons()
		{
			try
			{
				return Result.Success(_complaintsRepository.GetDriverComplaintPopularReasons(_unitOfWork)
					.Select(x => _driverComplaintReasonConverter.ConvertToAPIDriverComplaintReason(x)));
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка получения данных: {ExceptionMessage}", ex.Message);
				return Result.Failure<IEnumerable<DriverComplaintReasonDto>>(Vodovoz.Errors.Common.Repository.DataRetrievalError);
			}
		}
	}
}
