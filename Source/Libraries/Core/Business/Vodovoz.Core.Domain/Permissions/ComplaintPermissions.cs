using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class ComplaintPermissions
	{
		/// <summary>
		/// Изменение классификации рекламации
		/// </summary>
		[Display(
			Name = "Изменение классификации рекламации",
			Description = "Можно редактировать объект и вид недовоза в рекламации")]
		public static string CanEditComplaintClassification => "can_edit_complaint_classification";

		/// <summary>
		/// Можно создавать дубликаты рекламаций
		/// </summary>
		[Display(
			Name = "Можно создавать дубликаты рекламаций",
			Description = "Пользователь может создавать дубликаты рекламаций")]
		public static string CanCreateDuplicateComplaints => nameof(CanCreateDuplicateComplaints);
	}
}
