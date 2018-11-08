using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSOrmProject;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки акта передачи склада",
		Nominative = "строка акта передачи склада")]
	public class ShiftChangeWarehouseDocumentItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual ShiftChangeWarehouseDocument Document { get; set; }

		Nomenclature nomenclature;

		[Required(ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField(ref nomenclature, value, () => Nomenclature); }
		}

		decimal amountInDB;

		[Display(Name = "Количество по базе")]
		public virtual decimal AmountInDB {
			get { return amountInDB; }
			set { SetField(ref amountInDB, value, () => AmountInDB); }
		}

		decimal amountInFact;

		[Display(Name = "Количество по базе")]
		[PropertyChangedAlso("SumOfDamage")]
		public virtual decimal AmountInFact {
			get { return amountInFact; }
			set { SetField(ref amountInFact, value, () => AmountInFact); }
		}

		string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField(ref comment, value, () => Comment); }
		}

		[Display(Name = "Сумма ущерба")]
		public virtual decimal SumOfDamage {
			get {
				if(Difference > 0)
					return 0;
				else
					return Nomenclature.SumOfDamage * Math.Abs(Difference);
			}
		}

		#region Расчетные

		public virtual string Title {
			get {
				return String.Format("[{2}] {0} - {1}",
					Nomenclature.Name,
									 Nomenclature.Unit.MakeAmountShortStr(AmountInFact),
					Document.Title);
			}
		}

		public virtual decimal Difference {
			get {
				return AmountInFact - AmountInDB;
			}
		}

		#endregion
	}
}
