using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	/// <summary>
	/// Права телефон
	/// </summary>
	public static class Phone
	{
		/// <summary>
		/// Пользователь может архивировать/удалять телефон, на который зарегистрирован пользователь сайта/приложения
		/// </summary>
		[Display(
			Name = "Пользователь может архивировать/удалять телефон, на который зарегистрирован пользователь сайта/приложения",
			Description = "Пользователь может архивировать/удалять телефон, на который зарегистрирован пользователь сайта/приложения")]
		public static string CanArchiveOrDeleteExternalUserPhone => $"{nameof(Phone)}.{nameof(CanArchiveOrDeleteExternalUserPhone)}";
	}
}
