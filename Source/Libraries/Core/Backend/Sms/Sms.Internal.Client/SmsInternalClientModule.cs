using Autofac;

namespace Sms.Internal.Client
{
	public class SmsInternalClientModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SmsClientChannelFactory>()
				.AsSelf()
				.AsImplementedInterfaces()
				.InstancePerLifetimeScope();
		}
	}
}
