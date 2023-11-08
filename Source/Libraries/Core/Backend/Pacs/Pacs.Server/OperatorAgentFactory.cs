using QS.DomainModel.UoW;
using System;

namespace Pacs.Server
{
	public class OperatorAgentFactory : IOperatorAgentFactory
	{
		private readonly IPacsSettings _pacsSettings;
		private readonly IOperatorRepository _operatorRepository;
		private readonly IOperatorNotifier _operatorNotifier;
		private readonly IPhoneController _phoneController;
		private readonly IUnitOfWorkFactory _uowFactory;

		public OperatorAgentFactory(
			IPacsSettings pacsSettings,
			IOperatorRepository operatorRepository,
			IOperatorNotifier operatorNotifier,
			IPhoneController phoneController,
			IUnitOfWorkFactory uowFactory)
		{
			_pacsSettings = pacsSettings ?? throw new ArgumentNullException(nameof(pacsSettings));
			_operatorRepository = operatorRepository;
			_operatorNotifier = operatorNotifier;
			_phoneController = phoneController;
			_uowFactory = uowFactory;
		}

		public OperatorAgent CreateOperatorAgent(int operatorId)
		{
			return new OperatorAgent(operatorId,
				_pacsSettings,
				_operatorRepository,
				_operatorNotifier,
				_phoneController,
				_uowFactory);
		}
	}
}
