using System;
using Vodovoz.Domain.Client;
using PaymentTypeV1 = RobotMiaApi.Contracts.Requests.V1.PaymentType;

namespace RobotMiaApi.Extensions.Mapping
{
	/// <summary>
	/// Расширение функционала <see cref="PaymentTypeV1"/>
	/// </summary>
	public static class PaymentTypeExtensions
	{
		/// <summary>
		/// Маппинг типа оплаты в <see cref="PaymentType"/>
		/// </summary>
		/// <param name="paymentType"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static PaymentType MapToPaymentType(this PaymentTypeV1 paymentType)
		{
			return paymentType switch
			{
				PaymentTypeV1.Cash => PaymentType.Cash,
				PaymentTypeV1.SmsQR => PaymentType.SmsQR,
				_ => throw new ArgumentException($"Тип оплаты {paymentType} не поддерживается", nameof(paymentType))
			};
		}
	}
}
