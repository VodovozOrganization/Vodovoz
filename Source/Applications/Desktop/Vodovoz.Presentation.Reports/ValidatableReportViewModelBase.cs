using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Vodovoz.Presentation.Reports
{
	public abstract class ValidatableReportViewModelBase : ReportParametersViewModelBase, IValidatableObject
	{
		private readonly IValidator _validator;

		public ValidatableReportViewModelBase(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IValidator validator
			) : base(rdlViewerViewModel, reportInfoFactory)
		{
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
		}

		public abstract IEnumerable<ValidationResult> Validate(ValidationContext validationContext);

		protected virtual void GenerateReport()
		{
			var isValid = _validator.Validate(this);
			if(isValid)
			{
				LoadReport();
			}
		}
	}
}
