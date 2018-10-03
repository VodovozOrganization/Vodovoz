using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QSOrmProject;

namespace Vodovoz.Domain.Orders
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Masculine,
				NominativePlural = "виновные в недовозе",
				Nominative = "виновный в недовозе",
				Prepositional = "виновном в недовозе",
				PrepositionalPlural = "виновных в недовозе"
			   )
	]

	public class GuiltyInUndelivery : BusinessObjectBase<GuiltyInUndelivery>, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		UndeliveredOrder undeliveredOrder;
		[Display(Name = "Недовоз")]
		public virtual UndeliveredOrder UndeliveredOrder {
			get { return undeliveredOrder; }
			set { SetField(ref undeliveredOrder, value, () => UndeliveredOrder); }
		}

		GuiltyTypes guiltySide;
		[Display(Name = "Виновная сторона")]
		public virtual GuiltyTypes GuiltySide {
			get { return guiltySide; }
			set { SetField(ref guiltySide, value, () => GuiltySide); }
		}

		Subdivision guiltyDepartment;
		[Display(Name = "Виновный отдел ВВ")]
		public virtual Subdivision GuiltyDepartment {
			get { return guiltyDepartment; }
			set { SetField(ref guiltyDepartment, value, () => GuiltyDepartment); }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			if(GuiltySide == GuiltyTypes.Department && GuiltyDepartment == null)
				return GuiltySide.GetEnumTitle();
			if(GuiltySide == GuiltyTypes.Department && GuiltyDepartment != null)
				return "Отд: " + GuiltyDepartment.Name;
			return GuiltySide.GetEnumTitle();
		}

		public override bool Equals(object obj)
		{
			if(obj == null || this.GetType() != obj.GetType())
				return false;

			GuiltyInUndelivery guilty = (GuiltyInUndelivery)obj;
			bool result = this.UndeliveredOrder.Id == guilty.UndeliveredOrder.Id
			                  && this.GuiltySide == guilty.GuiltySide
			                  && this.GuiltyDepartment?.Id == guilty.GuiltyDepartment?.Id;
			return result;
		}

		public static bool operator ==(GuiltyInUndelivery x, GuiltyInUndelivery y)
		{
			return x.Equals(y);
		}

		public static bool operator !=(GuiltyInUndelivery x, GuiltyInUndelivery y)
		{
			return !(x == y);
		}

		public override int GetHashCode()
		{
			int result = 0;
			result += 31 * result + this.UndeliveredOrder.Id.GetHashCode();
			result += 31 * result + this.GuiltySide.GetHashCode();
			result += 31 * result + (this.GuiltyDepartment != null ? this.GuiltyDepartment.Id.GetHashCode() : 0);

			return result;
		}

		#endregion
	}

	/// <summary>
	/// ВНИМАНИЕ! При добавлении эл-ов необходимо так же добавить их в SQL запрос в
	/// журнале недовозов (UndeliveredOrdersVM.cs).
	/// </summary>
	public enum GuiltyTypes
	{
		[Display(Name = "Неизвестно")]
		Unknown,
		[Display(Name = "Клиент")]
		Client,
		[Display(Name = "Водитель")]
		Driver,
		[Display(Name = "Отдел ВВ")]
		Department,
		[Display(Name = "Мастер СЦ")]
		ServiceMan,
		[Display(Name = "Нет (не недовоз)")]
		None
	}

	public class GuiltyInUndeliveryGuiltySideStringType : NHibernate.Type.EnumStringType
	{
		public GuiltyInUndeliveryGuiltySideStringType() : base(typeof(GuiltyTypes))
		{
		}
	}
}