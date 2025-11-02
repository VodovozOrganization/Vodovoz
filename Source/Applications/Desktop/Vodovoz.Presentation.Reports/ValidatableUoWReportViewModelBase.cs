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
			if(uowFactory is null)
			{
				throw new ArgumentNullException(nameof(uowFactory));
			}

			UoW = uowFactory.CreateWithoutRoot();
		}

		public virtual IUnitOfWork UoW { get; protected set; }

		public virtual void Dispose()
		{
			UoW?.Dispose();
		}
	}
}
