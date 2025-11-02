using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		GenitivePlural = "типов дополнительного условия МЛ",
		NominativePlural = "типы дополнительного условия МЛ",
		Nominative = "тип дополнительного условия МЛ")]
	public class RouteListSpecialConditionType : PropertyChangedBase, IDomainObject
	{
		private string _name;

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		public static int OrdersHasComments => 1;

		public static int OrdersRequireTerminal => 2;

		public static int OrdersRequireTrifle => 3;

		public static int RouteListRequireAdditionalLoading => 4;

		public static int EquipmentCheckRequired => 5;
	}
}
