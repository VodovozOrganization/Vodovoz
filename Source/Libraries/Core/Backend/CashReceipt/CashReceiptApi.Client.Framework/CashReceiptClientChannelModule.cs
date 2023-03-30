using Autofac;

namespace CashReceiptApi.Client.Framework
{
	public class CashReceiptClientChannelModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CashReceiptClientChannelFactory>()
				.AsSelf()
				.InstancePerLifetimeScope();
		}
	}
}
