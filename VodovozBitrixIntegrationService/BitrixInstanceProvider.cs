using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using BitrixIntegration;
using Vodovoz.Services;

namespace VodovozBitrixIntegrationService
{
	public class BitrixInstanceProvider : IInstanceProvider
	{
		private readonly IBitrixServiceSettings bitrixServiceSettings;

		public BitrixInstanceProvider(IBitrixServiceSettings bitrixServiceSettings)
		{
			this.bitrixServiceSettings = bitrixServiceSettings ?? throw new ArgumentNullException(nameof(bitrixServiceSettings));
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext)
		{
			return new BitrixService(bitrixServiceSettings);
		}

		public object GetInstance(InstanceContext instanceContext, Message message)
		{
			return GetInstance(instanceContext);
		}

		public void ReleaseInstance(InstanceContext instanceContext, object instance)
		{
		}

		#endregion IInstanceProvider implementation
	}
}
