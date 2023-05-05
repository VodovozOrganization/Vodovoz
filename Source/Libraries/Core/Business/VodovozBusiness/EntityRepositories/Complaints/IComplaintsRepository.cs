﻿using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Complaints;
using Vodovoz.EntityRepositories.Complaints.ComplaintResults;

namespace Vodovoz.EntityRepositories.Complaints
{
	public interface IComplaintsRepository
	{
		IList<object[]> GetGuiltyAndCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null);
		int GetUnclosedComplaintsCount(IUnitOfWork uow, bool? withOverdue = null, DateTime? start = null, DateTime? end = null);

		/// <summary>
		/// Возвращает список id незакрытых рекламаций, в которых подключена дискуссия для указанного
		/// отдела, но комментарии в ней отсутствуют
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="subdivisionId">id отдела для которого выполняется поиск рекламаций</param>
		/// <returns>Список id рекламаций</returns>
		IList<int> GetUnclosedWithNoCommentsComplaintIdsBySubdivision(IUnitOfWork uow, int subdivisionId);

		IEnumerable<DriverComplaintReason> GetDriverComplaintReasons(IUnitOfWork unitOfWork);
        IEnumerable<DriverComplaintReason> GetDriverComplaintPopularReasons(IUnitOfWork unitOfWork);
		DriverComplaintReason GetDriverComplaintReasonById(IUnitOfWork unitOfWork, int driverComplaintReasonId);
        ComplaintSource GetComplaintSourceById(IUnitOfWork unitOfWork, int complaintSourceId);
    }
}
