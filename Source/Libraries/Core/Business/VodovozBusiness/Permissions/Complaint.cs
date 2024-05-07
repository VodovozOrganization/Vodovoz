using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	public static partial class Complaint
	{
		/// <summary>
		/// Можно создавать дубликаты рекламаций
		/// </summary>
		[Display(
			Name = "Можно создавать дубликаты рекламаций",
			Description = "Пользователь может создавать дубликаты рекламаций")]
		public static string CanCreateDuplicateComplaints => nameof(CanCreateDuplicateComplaints);
	}
}
