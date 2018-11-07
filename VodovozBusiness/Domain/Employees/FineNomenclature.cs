using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSOrmProject;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Employees
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "номенклатура штрафа",
		Nominative = "номенклатура штрафа")]
	public class FineNomenclature: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual Fine Fine { get; set; }

		Nomenclature nomenclature;

		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set {
				SetField (ref nomenclature, value, () => Nomenclature);
			}
		}

		decimal amount;

		[Display (Name = "Количество")]
		public virtual decimal Amount {
			get { return amount; }
			set {
				SetField (ref amount, value, () => Amount);
			}
		}

		public virtual string Title {
			get{
				return String.Format("{0} - {1}", 
					Nomenclature.Name, 
					Nomenclature.Unit.MakeAmountShortStr(Amount));
			}
		}

	}

}

