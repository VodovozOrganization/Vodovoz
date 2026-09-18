using Edo.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VodovozBusiness.Services.Receipts;

namespace Vodovoz.Core.Application.Receipts.Correction
{
	public static class ReceiptCorrectionDependencyInjection
	{
		public static IServiceCollection AddReceiptCorrectionServices(this IServiceCollection services)
		{
			services
				.AddScoped<IOrderReceiptCorrectionHandler, OrderReceiptCorrectionHandler>()
				.AddScoped<IFiscalOrderSnapshotBuilder, FiscalOrderSnapshotBuilder>()
				.AddScoped<ReceiptCorrectionEdoTaskFactory>()
				.AddScoped<ReceiptCorrectionFiscalDocumentBuilder>()
				.AddScoped<IFiscalChangeDetector, FiscalChangeDetector>()
				.AddScoped<IReceiptCorrectionScenarioClassifier, ReceiptCorrectionScenarioClassifier>()
				.AddScoped<IReceiptCorrectionExplanatoryNoteBuilder, ReceiptCorrectionExplanatoryNoteBuilder>();

			services.TryAddScoped<IEdoOrderContactProvider, EdoOrderContactProvider>();

			return services;
		}
	}
}
