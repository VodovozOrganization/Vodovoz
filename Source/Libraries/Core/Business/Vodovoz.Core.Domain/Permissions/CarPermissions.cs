using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public class CarPermissions
	{
		/// <summary>
		/// Пользователь может менять состав автопарка компании
		/// </summary>
		[Display(
			Name = "Пользователь может менять состав автопарка компании",
			Description = "Позволяет пользователю архивировать/разархивировать машины компании и в раскате," +
				" а также менять им принадлежность и создавать новые")]
		public static string CanChangeCompositionCompanyTransportPark => "Car.CanChangeCompositionCompanyTransportPark";
	}
}
