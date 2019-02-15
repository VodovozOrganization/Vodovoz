using System;
using QS.DomainModel.Entity;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain.Cash
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "статьи дохода",
		Nominative = "статья дохода")]
	[EntityPermission]
	public class IncomeCategory : PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Название статьи должно быть заполнено.")]
		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		IncomeInvoiceDocumentType incomeDocumentType;
		/// <summary>
		/// Тип приходного ордера для котором возможно будет выбрать эту категорию
		/// </summary>
		[Required(ErrorMessage = "Должно быть заполнен тип приходного ордера.")]
		[Display(Name = "Тип приходного ордера")]
		public virtual IncomeInvoiceDocumentType IncomeDocumentType {
			get { return incomeDocumentType; }
			set { SetField(ref incomeDocumentType, value, () => IncomeDocumentType); }
		}

		#endregion

		public IncomeCategory ()
		{
			Name = String.Empty;
		}
	}
}

