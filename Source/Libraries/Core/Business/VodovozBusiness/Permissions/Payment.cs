using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	/// <summary>
	/// Права Платежи
	/// </summary>
	public static partial class Payment
	{
		/// <summary>
		/// Право создания ручных платежей
		/// </summary>
		[Display(
			Name = "Пользователь может создавать ручные платежи для разноса ретробонусов и ввода остатков",
			Description = "Пользователь может создавать ручные платежи для разноса ретробонусов и ввода остатков")]
		public static string CanCreateNewManualPaymentFromBankClient => "can_create_new_manual_payment_from_bank_client";
		/// <summary>
		/// Возможность отмены вручную созданных платежей из банк-клиента
		/// </summary>
		[Display(
			Name = "Отмена вручную созданных платежей из банк-клиента",
			Description = "Доступ к кнопке Отмена платежа в журнале платежей из банк-клиента")]
		public static string CanCancelManualPaymentFromBankClient => "can_cancel_manual_payment_from_bank_client";
		/// <summary>
		/// Возможность разнесения остатка на другого КА
		/// </summary>
		[Display(
			Name = "Разнос остатка на другого клиента",
			Description = "Возможность разнести остаток средств в ручном распределении на другого клиента")]
		public static string CanAllocatePaymentToSeveralClients =>
			$"{nameof(Payment)}.can_allocate_payment_to_several_clients";
	}
}
