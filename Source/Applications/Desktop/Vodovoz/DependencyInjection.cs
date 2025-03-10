using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Pacs.Admin.Client;
using Pacs.Admin.Client.Consumers;
using Pacs.Admin.Client.Consumers.Definitions;
using Pacs.Calls.Consumers;
using Pacs.Calls.Consumers.Definitions;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using Pacs.Operators.Client;
using Pacs.Operators.Client.Consumers;
using Pacs.Operators.Client.Consumers.Definitions;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.Entity.PresetPermissions;
using QS.Services;
using System;
using Vodovoz.Core;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Infrastructure.Print;
using Vodovoz.PermissionExtensions;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.Presentation.Views;
using Edo.Transport;
using TrueMarkApi.Client;

namespace Vodovoz
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddWaterDeliveryDesktop(this IServiceCollection services) =>
			services.AddVodovozViewModels()
				.AddPresentationViews()
				.AddDocumentPrinter()
				.AddTrueMarkApiClient();

		public static IServiceCollection AddPermissionValidation(this IServiceCollection services)
		{
			services.AddSingleton<IPermissionService, PermissionService>()
				.AddSingleton<IEntityExtendedPermissionValidator, EntityExtendedPermissionValidator>()
				.AddSingleton<IWarehousePermissionValidator, WarehousePermissionValidator>()
				.AddSingleton<IPermissionExtensionStore>(sp => PermissionExtensionSingletonStore.GetInstance())
				.AddScoped<IDocumentPrinter, DocumentPrinter>();

			services.AddSingleton<IEntityPermissionValidator, Vodovoz.Domain.Permissions.EntityPermissionValidator>()
				.AddSingleton<IPresetPermissionValidator, Vodovoz.Domain.Permissions.HierarchicalPresetPermissionValidator>();

			return services;
		}

		public static IServiceCollection AddDocumentPrinter(this IServiceCollection services) =>
			services.AddScoped<IDocumentPrinter, DocumentPrinter>();

		public static IServiceCollection AddPacs(this IServiceCollection services)
		{
			services.AddPacsOperatorClient()
				.AddSingleton<SettingsConsumer>()
				.AddSingleton<IObservable<SettingsEvent>>(ctx => ctx.GetRequiredService<SettingsConsumer>())
				.AddScoped<PacsEndpointsConnector>()
				.AddScoped<MessageEndpointConnector>()
				.AddSingleton<OperatorStateAdminConsumer>()
				.AddSingleton<IObservable<OperatorState>>(ctx => ctx.GetRequiredService<OperatorStateAdminConsumer>());

			services.AddHttpClient<IAdminClient, AdminClient>(c =>
			{
				c.DefaultRequestHeaders.Clear();
				c.DefaultRequestHeaders.Add("Accept", "application/json");
			});

			services.AddPacsMassTransitNotHosted(
				(context, rabbitCfg) =>
				{
					rabbitCfg.AddPacsBaseTopology(context);
					rabbitCfg.AddEdoTopology(context);
				},
				(busCfg) =>
				{
					//Оператор
					busCfg.AddConsumer<OperatorStateConsumer, OperatorStateConsumerDefinition>();
					busCfg.AddConsumer<OperatorsOnBreakConsumer, OperatorsOnBreakConsumerDefinition>();
					busCfg.AddConsumer<OperatorSettingsConsumer, OperatorSettingsConsumerDefinition>();

					//Админ
					busCfg.AddConsumer<OperatorStateAdminConsumer, OperatorStateAdminConsumerDefinition>();
					busCfg.AddConsumer<SettingsConsumer, SettingsConsumerDefinition>();
					busCfg.AddConsumer<PacsCallEventConsumer, PacsCallEventConsumerDefinition>();
				}
				//Exclude необходим для отложенного запуска конечной точки, или отмены запуска по условию
				//При этом добавление определения потребителя в конфигурации обязательно
				, (filter) =>
				{
					filter.Exclude<SettingsConsumer>();
					filter.Exclude<OperatorSettingsConsumer>();
					filter.Exclude<OperatorStateAdminConsumer>();
					filter.Exclude<OperatorStateConsumer>();
					filter.Exclude<OperatorsOnBreakConsumer>();
					filter.Exclude<PacsCallEventConsumer>();
				}
			);

			return services;
		}
	}
}
