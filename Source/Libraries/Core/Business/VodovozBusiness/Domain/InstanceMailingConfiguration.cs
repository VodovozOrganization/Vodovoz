using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
