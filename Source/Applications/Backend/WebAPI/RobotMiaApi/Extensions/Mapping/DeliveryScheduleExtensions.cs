﻿using RobotMiaApi.Contracts.Responses.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;

namespace RobotMiaApi.Extensions.Mapping
{
	/// <summary>
	/// Расширение функционала <see cref="DeliverySchedule"/>
	/// </summary>
	public static class DeliveryScheduleExtensions
	{
		/// <summary>
		/// Маппинг интервала доставки в <see cref="DeliveryIntervalDto"/>
		/// </summary>
		/// <param name="deliverySchedule"></param>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static DeliveryIntervalDto MapToDeliveryIntervalDtoV1(this DeliverySchedule deliverySchedule, DateTime dateTime)
			=> new DeliveryIntervalDto
			{
				Id = deliverySchedule.Id,
				From = dateTime.Add(deliverySchedule.From),
				To = dateTime.Add(deliverySchedule.To),
			};

		/// <summary>
		/// Маппинг интервалов доставки в IEnumerable <see cref="DeliveryIntervalDto"/>
		/// </summary>
		/// <param name="deliverySchedules"></param>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static IEnumerable<DeliveryIntervalDto> MapToDeliveryIntervalDtoV1(this IEnumerable<DeliverySchedule> deliverySchedules, DateTime dateTime)
			=> deliverySchedules.Select(ds => ds.MapToDeliveryIntervalDtoV1(dateTime));
	}
}
