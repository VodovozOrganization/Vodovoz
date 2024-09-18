using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;

namespace Vodovoz.Core.Domain.Organizations
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "Данные по направлениям деятельностей",
		Nominative = "Данные по направлению деятельности",
		GenitivePlural = "Данные по направлению деятельности")]
	public class BusinessActivitySummary : PropertyChangedBase, IDomainObject, IBankStatementParsingResult
	{
		private decimal? _total;
		private BusinessActivity _businessActivity;
		private FundsSummary _fundsSummary;
		private IObservableList<BusinessAccountSummary> _businessAccountsSummary = new ObservableList<BusinessAccountSummary>();

		public virtual int Id { get; set; }

		[Display(Name = "Баланс")]
		public virtual decimal? Total
		{
			get => _total;
			set => SetField(ref _total, value);
		}

		[Display(Name = "Направление деятельности")]
		public virtual BusinessActivity BusinessActivity
		{
			get => _businessActivity;
			set => SetField(ref _businessActivity, value);
		}

		[Display(Name = "Форма денежных средств")]
		public virtual FundsSummary FundsSummary
		{
			get => _fundsSummary;
			set => SetField(ref _fundsSummary, value);
		}

		[Display(Name = "Данные по расчетным счетам")]
		public virtual IObservableList<BusinessAccountSummary> BusinessAccountsSummary
		{
			get => _businessAccountsSummary;
			set => SetField(ref _businessAccountsSummary, value);
		}

		public virtual string Name => BusinessActivity?.Name;
		public virtual string AccountNumber => string.Empty;
		public virtual string Bank => string.Empty;
		
		public static BusinessActivitySummary Create(BusinessActivity businessActivity, FundsSummary fundsSummary)
			=> new BusinessActivitySummary
			{
				BusinessActivity = businessActivity,
				FundsSummary = fundsSummary
			};
	}
}
