﻿using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.Services;

namespace Vodovoz.EntityRepositories.Logistic
{
	/// <summary>
	/// Интерфейс для запросов по трэку водителя
	/// </summary>
	public interface ITrackRepository
	{
		Track GetTrackByRouteListId(IUnitOfWork unitOfWork, int routeListId);
		IList<TrackPoint> GetPointsForTrack(IUnitOfWork uow, int trackId);
		IList<TrackPoint> GetPointsForRouteList(IUnitOfWork uow, int routeListId);
		IList<DriverPosition> GetLastPointForRouteLists(IUnitOfWork uow, int[] routeListsIds, DateTime? beforeTime = null);
		IList<DriverPosition> GetLastRouteListFastDeliveryTrackPoints(IUnitOfWork uow, int[] routeListsIds, TimeSpan timeSpanDisconnected, DateTime? beforeTime = null);
		IList<DriverPositionWithFastDeliveryRadius> GetLastPointForRouteListsWithRadius(IUnitOfWork uow, int[] routeListsIds, DateTime? beforeTime = null);
		IList<DriverPositionWithFastDeliveryRadius> GetLastRouteListFastDeliveryTrackPointsWithRadius(IUnitOfWork uow, int[] routeListsIds, TimeSpan timeSpanDisconnected, DateTime? beforeTime = null);
		DateTime GetMinTrackPointDate(IUnitOfWork uow);
		bool TrackPointsExists(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo);
		/// <summary>
		/// Получение последних координат трэка водителя по онлайн заказу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="externalOnlineOrder">Id онлайн заказа в ИПЗ</param>
		/// <returns>Последние координаты водителя</returns>
		PointCoordinates GetDriverLastCoordinateFromOnlineOrder(IUnitOfWork uow, Guid externalOnlineOrder);
		void DeleteTrackPoints(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo);
	}
}
