using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static class RecomendationSettings
	{
		/// <summary>
		/// Доступ к изменению настроек рекомендаций <br/>
		/// При наличии права настройки рекомендаций доступны для изменения
		/// </summary>
		[Display(
			Name = "Доступ к изменению настроек рекомендаций",
			Description = "При наличии права настройки рекомендаций доступны для изменения")]
		public static string CanChangeSettings => "Goods.Recomendations.CanChangeSettings";
	}
}
