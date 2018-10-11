using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSOrmProject;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Store
{
	[OrmSubject (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки шаблона пересортицы",
		Nominative = "строка шаблона пересортицы")]
	public class RegradingOfGoodsTemplateItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual RegradingOfGoodsTemplate Template { get; set; }

		Nomenclature nomenclatureOld;

		[Required (ErrorMessage = "Старая номенклатура должна быть заполнена.")]
		[Display (Name = "Старая номенклатура")]
		public virtual Nomenclature NomenclatureOld {
			get { return nomenclatureOld; }
			set {
				SetField (ref nomenclatureOld, value, () => NomenclatureOld);
			}
		}

		Nomenclature nomenclatureNew;

		[Required (ErrorMessage = "Новая номенклатура должна быть заполнена.")]
		[Display (Name = "Новая номенклатура")]
		public virtual Nomenclature NomenclatureNew {
			get { return nomenclatureNew; }
			set {
				SetField (ref nomenclatureNew, value, () => NomenclatureNew);
			}
		}

		#region Расчетные

		public virtual string Title {
			get{
				return String.Format("{0} -> {1}", 
					NomenclatureOld.Name, 
					NomenclatureNew.Name);
			}
		}

		#endregion
	}
}

