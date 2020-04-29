using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Client
{
	public class SpecialNomenclature : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private Nomenclature nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value);
		}

		private int specialId;
		[Display(Name = "Код ТМЦ")]
		public virtual int SpecialId {
			get => specialId;
			set => SetField(ref specialId, value);
		}

		private Counterparty counterparty;
		[Display(Name = "Клиент")]
		public virtual Counterparty Counterparty {
			get => counterparty;
			set => SetField(ref counterparty, value);
		}

	}
}
