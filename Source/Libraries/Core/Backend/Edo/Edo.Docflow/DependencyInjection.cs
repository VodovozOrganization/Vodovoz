using Edo.Docflow;
using Edo.Docflow.Converters;
using Edo.Docflow.Factories;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySqlConnector;
using QS.DomainModel.UoW;
using System.Reflection;
using Vodovoz.Core.Domain.Controllers;
using VodovozBusiness.Converters;

namespace Edo.Documents
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoDocflowServices(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services.TryAddScoped<DocflowHandler>();
			services.TryAddScoped<OrderUpdInfoFactory>();
			services.TryAddScoped<TransferOrderUpdInfoFactory>();
			services.TryAddScoped<IInfoForCreatingEdoEquipmentTransferFactory, InfoForCreatingEdoEquipmentTransferFactory>();
			services.TryAddScoped<ICounterpartyEdoAccountEntityController, CounterpartyEdoAccountEntityController>();

			services.TryAddScoped<IOrderConverter, OrderConverter>();
			services.TryAddScoped<ICounterpartyConverter, CounterpartyConverter>();
			services.TryAddScoped<IDeliveryPointConverter, DeliveryPointConverter>();
			services.TryAddScoped<ICounterpartyContractConverter, CounterpartyContractConverter>();
			services.TryAddScoped<IOrderItemConverter, OrderItemConverter>();
			services.TryAddScoped<Vodovoz.Converters.ISpecialNomenclatureConverter, Vodovoz.Converters.SpecialNomenclatureConverter>();
			services.TryAddScoped<IPersonTypeConverter, PersonTypeConverter>();
			services.TryAddScoped<IReasonForLeavingConverter, ReasonForLeavingConverter>();
			services.TryAddScoped<ICargoReceiverSourceConverter, CargoReceiverSourceConverter>();
			services.TryAddScoped<IOrganizationConverter, OrganizationConverter>();
			services.TryAddScoped<INomenclatureConverter, NomenclatureConverter>();
			services.TryAddScoped<Vodovoz.Converters.IMeasurementUnitConverter, Vodovoz.Converters.MeasurementUnitConverter>();
			services.TryAddScoped<INomenclatureCategoryConverter, NomenclatureCategoryConverter>();
			// Перенести в Edo.InformalOrderDocuments
			services.TryAddScoped<IPrintableDocumentSaver>(sp =>
			{
				var connectionStringBuilder = sp.GetRequiredService<MySqlConnectionStringBuilder>();
				return new PrintableDocumentSaver(connectionStringBuilder);
			});

			return services;
		}

		public static IServiceCollection AddEdoDocflow(this IServiceCollection services)
		{
			services.AddEdoDocflowServices();

			services.AddEdoMassTransit(configureBus: cfg =>
			{
				cfg.AddConsumers(Assembly.GetExecutingAssembly());
			});

			return services;
		}
	}
}
