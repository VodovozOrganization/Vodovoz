﻿using System;
using System.Text.Json.Serialization;
using TaxcomEdo.Contracts.OrdersWithoutShipment;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Необходимая информация для отправки счета без погрузки по ЭДО
	/// </summary>
	public abstract class InfoForCreatingBillWithoutShipmentEdo : InfoForCreatingDocumentEdoWithAttachment
	{
		/// <summary>
		/// Информация о счете без погрузки для ЭДО <see cref="OrderWithoutShipmentInfo"/>
		/// </summary>
		[JsonIgnore]
		public OrderWithoutShipmentInfo OrderWithoutShipmentInfo { get; set; }

		public void Initialize(OrderWithoutShipmentInfo orderWithoutShipmentInfo, FileData fileData)
		{
			OrderWithoutShipmentInfo = orderWithoutShipmentInfo;
			FileData = fileData;
			MainDocumentId = Guid.NewGuid();
		}

		public string GetBillWithoutShipmentInfoTitle()
		{
			if(OrderWithoutShipmentInfo is null)
			{
				return "Не заполнена информация о счете без отгрузки";
			}

			return OrderWithoutShipmentInfo.ToString();
		}
	}
}
