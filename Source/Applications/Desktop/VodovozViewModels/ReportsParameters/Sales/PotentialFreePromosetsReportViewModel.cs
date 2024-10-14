using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.Presentation.Reports;

namespace Vodovoz.ViewModels.ReportsParameters.Sales
{
	public class PotentialFreePromosetsReportViewModel : ValidatableUoWReportViewModelBase
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		private DateTime? _startDate;
		private DateTime? _endDate;
		private IEnumerable<PromosetReportNode> _promosets;

		public PotentialFreePromosetsReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IUnitOfWorkFactory uowFactory,
			IValidator validator
		) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			Title = "Отчет по потенциальным халявщикам";
			Identifier = "Client.PotentialFreePromosets";

			UoW = _uowFactory.CreateWithoutRoot();

			_promosets = (from ps in UoW.GetAll<PromotionalSet>()
								select new PromosetReportNode
								{
									Id = ps.Id,
									Name = ps.Name,
									Active = ps.PromotionalSetForNewClients,
								})
								   .ToList();

			GenerateReportCommand = new DelegateCommand(GenerateReport);
		}

		public DelegateCommand GenerateReportCommand;

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public virtual IEnumerable<PromosetReportNode> Promosets
		{
			get => _promosets;
			set => SetField(ref _promosets, value);
		}

		public override ReportInfo ReportInfo
		{
			get
			{
				var reportInfo = base.ReportInfo;
				reportInfo.UseUserVariables = true;
				return reportInfo;
			}
		}

		protected override Dictionary<string, object> Parameters
		{
			get
			{
				var parameters = new Dictionary<string, object>
					{
						{ "start_date", StartDate },
						{ "end_date", EndDate },
						{ "promosets", GetSelectedPromotionalSets() }
					};

				return parameters;
			}
		}

		private int[] GetSelectedPromotionalSets()
		{
			if(_promosets.Any(x => x.Active))
			{
				return _promosets.Where(x => x.Active).Select(x => x.Id).ToArray();
			}

			//если ни один промосет не выбран, необходимо выбрать все
			return _promosets.Select(x => x.Id).ToArray();
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(StartDate == null || EndDate == null)
			{
				yield return new ValidationResult("Необходимо выбрать период.", new[] { nameof(StartDate), nameof(EndDate) });
			}
		}
	}

	public class PromosetReportNode : PropertyChangedBase
	{
		private bool _active;
		public virtual bool Active
		{
			get => _active;
			set => SetField(ref _active, value);
		}

		public int Id { get; set; }

		public string Name { get; set; }
	}
}
