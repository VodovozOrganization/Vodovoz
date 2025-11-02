using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Операция распределения платежа
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "Передвижения безнала(сокр)",
		Nominative = "Передвижение безнала(сокр)")]
	public class CashlessMovementOperationEntity : OperationBase
	{
		private decimal _income;
		private decimal _expense;
		private AllocationStatus _cashlessMovementOperationStatus;
		private CounterpartyEntity _counterparty;
		private OrganizationEntity _organization;

		/// <summary>
		/// Приход
		/// </summary>
		[Display(Name = "Приход")]
		public virtual decimal Income
		{
			get => _income;
			set => SetField(ref _income, value);
		}

		/// <summary>
		/// Расход
		/// </summary>
		[Display(Name = "Расход")]
		public virtual decimal Expense
		{
			get => _expense;
			set => SetField(ref _expense, value);
		}

		/// <summary>
		/// Контрагент
		/// </summary>
		[Display(Name = "Контрагент")]
		public virtual CounterpartyEntity Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}
		
		/// <summary>
		/// Организация
		/// </summary>
		[Display(Name = "Организация")]
		public virtual OrganizationEntity Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}
		
		/// <summary>
		/// Статус распределения
		/// </summary>
		public virtual AllocationStatus CashlessMovementOperationStatus
		{
			get => _cashlessMovementOperationStatus;
			set => SetField(ref _cashlessMovementOperationStatus, value);
		}
	}
}
