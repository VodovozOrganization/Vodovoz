using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Operations
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "передвижения безнала",
		Nominative = "передвижение безнала")]
	public class CashlessMovementOperation : OperationBase
	{
		decimal _income;
		decimal _expense;
		Counterparty _counterparty;
		Organization _organization;

		[Display(Name = "Приход")]
		public virtual decimal Income
		{
			get => _income;
			set => SetField(ref _income, value);
		}

		[Display(Name = "Расход")]
		public virtual decimal Expense
		{
			get => _expense;
			set => SetField(ref _expense, value);
		}

		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}
		
		[Display(Name = "Организация")]
		public virtual Organization Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}
	}
}
