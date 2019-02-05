using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSOrmProject;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "теги",
		Nominative = "тег")]
	public class Tag : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required(ErrorMessage = "Название тега должно быть заполнено.")]
		[Display(Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		string colorText;

		[Display(Name = "Цвет строки")]
		public virtual string ColorText {
			get { return colorText; }
			set { SetField(ref colorText, value, () => ColorText); }
		}

		#endregion

		public Tag()
		{
			Name = String.Empty;
		}

		[Display(Name = "Цвет")]
		public virtual Gdk.Color Color {
			get {
				Gdk.Color color = new Gdk.Color();
				if(String.IsNullOrEmpty(ColorText)) {
					return Gdk.Color.Zero;
				}
				if(!Gdk.Color.Parse(ColorText, ref color)){
					throw new InvalidCastException("Ошибка в распознавании цвета тега");
				}
				return color;
			}
			set {
				ColorText =  String.Format("#{0:x4}{1:x4}{2:x4}", value.Red, value.Green, value.Blue);
			}
		}

		public static IUnitOfWorkGeneric<Tag> Create()
		{
			var uow = UnitOfWorkFactory.CreateWithNewRoot<Tag>();
			return uow;
		}
	}
}