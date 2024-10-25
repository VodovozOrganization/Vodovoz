using QS.DomainModel.UoW;
using QS.Report;
using QS.Report.ViewModels;
using QS.Validation;
using System;

namespace Vodovoz.Presentation.Reports
{
	public abstract class ValidatableUoWReportViewModelBase : ValidatableReportViewModelBase, IDisposable
	{
		public ValidatableUoWReportViewModelBase(
			RdlViewerViewModel rdlViewerViewModel,
			IReportInfoFactory reportInfoFactory,
			IValidator validator
			) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
		}
		
		protected ValidatableUoWReportViewModelBase(
			RdlViewerViewModel rdlViewerViewModel,
			IUnitOfWorkFactory uowFactory,
			IReportInfoFactory reportInfoFactory,
			IValidator validator
			) : base(rdlViewerViewModel, reportInfoFactory, validator)
		{
			UoW = (uowFactory ?? throw new ArgumentNullException(nameof(uowFactory))).CreateWithoutRoot();
		}

		public virtual IUnitOfWork UoW { get; set; }

		public virtual void Dispose()
		{
			UoW?.Dispose();
		}
	}
}
