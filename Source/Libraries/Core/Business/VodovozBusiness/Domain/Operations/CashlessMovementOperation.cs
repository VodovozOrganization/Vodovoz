using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Domain.Operations
{
	/// <summary>
	/// Операция распределения платежа
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "Передвижения безнала",
		Nominative = "Передвижение безнала")]
	public class CashlessMovementOperation : CashlessMovementOperationEntity
	{
		private Counterparty _counterparty;
		private Organization _organization;
		
		/// <summary>
		/// Контрагент
		/// </summary>
		[Display(Name = "Контрагент")]
		public virtual new Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}
		
		/// <summary>
		/// Организация
		/// </summary>
		[Display(Name = "Организация")]
		public virtual new Organization Organization
		{
			get => _organization;
			set => SetField(ref _organization, value);
		}
	}
}
