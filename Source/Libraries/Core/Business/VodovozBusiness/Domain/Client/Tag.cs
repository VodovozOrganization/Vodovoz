using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "теги",
		Nominative = "тег")]
	[EntityPermission]
	public class Tag : PropertyChangedBase, IDomainObject
	{
		private string _name = "";
		private string _colorText;

		#region Свойства

		public virtual int Id { get; set; }


		[Required(ErrorMessage = "Название тега должно быть заполнено.")]
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}


		[Display(Name = "Цвет строки")]
		public virtual string ColorText
		{
			get => _colorText;
			set => SetField(ref _colorText, value);
		}

		#endregion
	}
}
