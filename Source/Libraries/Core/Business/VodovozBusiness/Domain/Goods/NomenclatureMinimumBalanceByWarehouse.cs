using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.Domain.Goods
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "минимальные остатки на складе",
		Nominative = "минимальный остаток на складе"
	)]
	[HistoryTrace]

	public class NomenclatureMinimumBalanceByWarehouse : PropertyChangedBase, IDomainObject
	{

		private Nomenclature _nomenclature;
		private Warehouse _warehouse;
		private int _minimumBalance;

		public virtual int Id { get; set; }

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Склад")]
		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set => SetField(ref _warehouse, value);
		}

		[Display(Name = "Минимальный остаток")]
		public virtual int MinimumBalance
		{
			get => _minimumBalance;
			set => SetField(ref _minimumBalance, value);
		}
	}
}
