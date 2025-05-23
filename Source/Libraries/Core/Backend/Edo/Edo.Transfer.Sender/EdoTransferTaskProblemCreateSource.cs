using Edo.Problems.Custom;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Transfer.Sender
{
	public class EdoTransferTaskProblemCreateSource : EdoTaskProblemCustomSource
	{
		public override string Name => "TransferSendPreparer.CreateTransferOrder";

		public override string Message => "Не установлены требуемые параметры при создании заказа перемещения товаров";

		public override string Description => "При попытке создания заказа перемещения товаров не были переданы или переданы не корректные параметры значений";

		public override string Recommendation => "Проверьте правильность передачи параметров";

		public override EdoProblemImportance Importance => EdoProblemImportance.Problem;
	}
}
