using QS.DomainModel.Entity;

namespace VodovozInfrastructure.Configuration
{
	public class InstanceMailingConfiguration : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual string MessageBrokerHost { get; set; }
		public virtual string MessageBrokerVirtualHost { get; set; }
		public virtual int Port { get; set; }
		public virtual string MessageBrokerUsername { get; set; }
		public virtual string MessageBrokerPassword { get; set; }
		public virtual string EmailSendExchange { get; set; }
		public virtual string EmailSendKey { get; set; }
	}
}
