using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
				NominativePlural = "ответственные в недовозе",
				Nominative = "ответственный в недовозе",
				Prepositional = "ответственом в недовозе",
				PrepositionalPlural = "ответственных в недовозе"
			   )
	]
	[HistoryTrace]
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
		[Display(Name = "Ответственная сторона")]
		public virtual GuiltyTypes GuiltySide {
			get { return guiltySide; }
			set { SetField(ref guiltySide, value, () => GuiltySide); }
		}

		Subdivision guiltyDepartment;
		[Display(Name = "Ответственный отдел ВВ")]
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
			if(x is null)
				return y is null;
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
	/// журнале недовозов
	/// </summary>
	public enum GuiltyTypes
	{
		[Display(Name = "Клиент")]
		Client,
		[Display(Name = "Водитель")]
		Driver,
		[Display(Name = "Отдел ВВ")]
		Department,
		[Display(Name = "Мастер СЦ")]
		ServiceMan,
		[Display(Name = "Форс-мажор")]
		ForceMajor,
		[Display(Name = "Доставка за час")]
		DirectorLO,
		[Display(Name = "Довезли в тот же день")]
		DirectorLOCurrentDayDelivery,
		[Display(Name = "Автоотмена автопереноса")]
		AutoСancelAutoTransfer,
		[Display(Name = "Нет (не недовоз)")]
		None
	}
}
