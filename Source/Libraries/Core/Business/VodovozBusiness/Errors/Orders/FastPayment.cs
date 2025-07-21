using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Errors.Orders
{
	/// <summary>
	/// Ошибки при работе с СБП
	/// </summary>
	public static class FastPayment
	{
		/// <summary>
		/// Не зарегистрированная организация в Авангарде
		/// </summary>
		public static Error OrganizationNotRegisteredInAvangard =>
			new Error(
				typeof(FastPayment),
				nameof(OrganizationNotRegisteredInAvangard),
				"Организация не зарегистрирована в Авангарде, " +
				"нужно поменять организацию в заказе для отправки ссылки на оплату");
		
		/// <summary>
		/// Не подобран контракт или организация для заказа
		/// </summary>
		public static Error OrderContractNotFound =>
			new Error(
				typeof(FastPayment),
				nameof(OrderContractNotFound),
				"У заказа должен быть подобран договор с действующей организацией в Авангарде" +
				" для отправки ссылки на оплату");
	}
}
