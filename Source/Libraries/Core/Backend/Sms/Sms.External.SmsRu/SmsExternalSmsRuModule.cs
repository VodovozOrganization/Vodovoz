using Autofac;

namespace Sms.External.SmsRu
{
	public class SmsExternalSmsRuModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SmsRuConfiguration>()
				.AsSelf()
				.AsImplementedInterfaces()
				.InstancePerLifetimeScope();

			builder.RegisterType<SmsRuSendController>()
				.AsSelf()
				.AsImplementedInterfaces()
				.InstancePerLifetimeScope();
		}
	}
}
