using System;
using Vodovoz.Domain.Client;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.ChangingPaymentTypeByDriversReport
{
	public class ChangingPaymentTypeByDriversReportRow
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		public string OrderId { get; internal set; }

		/// <summary>
		/// Дата и время
		/// </summary>
		public string ChangeDateTime { get; internal set; }

		/// <summary>
		/// ФИО водителя
		/// </summary>
		public string DriverName { get; internal set; }

		/// <summary>
		/// Исходный способ оплаты DisplayName
		/// </summary>
		public string OriginalPaymentTypeDisplayName => GetPaymentDisplayName(OriginalPaymentTypeString);

		/// <summary>
		/// Новый способ оплаты DisplayName
		/// </summary>
		public string NewPaymentTypeDisplayName => GetPaymentDisplayName(NewPaymentTypeString);

		/// <summary>
		/// Сумма заказа
		/// </summary>
		public string OrderSum { get; internal set; }

		/// <summary>
		/// Исходный способ оплаты
		/// </summary>
		public string OriginalPaymentTypeString { get; internal set; }

		/// <summary>
		/// Новый способ оплаты
		/// </summary>
		public string NewPaymentTypeString { get; internal set; }

		/// <summary>
		/// Заголовок группировки?
		/// </summary>
		public bool IsTitle { get; internal set; }

		private string GetPaymentDisplayName(string paymentTypeString)
		{
			var isParsed = Enum.TryParse(paymentTypeString, out PaymentType paymentType);

			return isParsed ? paymentType.GetEnumDisplayName() : "";
		}
	}


}
