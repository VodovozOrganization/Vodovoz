using System;

namespace CustomerOrdersApi.Library.Common
{
	public static class Messages
	{
		public static string ErrorMessage => "Произошла ошибка, пожалуйста, попробуйте позже";
		public static string DuplicateOrderMessage(Guid externalOrderId) => $"Онлайн заказ с Id {externalOrderId} уже существует!";
	}
}
