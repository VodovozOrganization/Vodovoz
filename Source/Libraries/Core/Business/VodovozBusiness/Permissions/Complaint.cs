using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	/// <summary>
	/// Права заказы
	/// </summary>
	public static partial class Complaint
	{
		/// <summary>
		/// Изменение классификации рекламации
		/// </summary>
		[Display(
			Name = "Изменение классификации рекламации",
			Description = "Можно редактировать объект и вид недовоза в рекламации")]
		public static string CanEditComplaintClassification => "can_edit_complaint_classification";

	}
}
