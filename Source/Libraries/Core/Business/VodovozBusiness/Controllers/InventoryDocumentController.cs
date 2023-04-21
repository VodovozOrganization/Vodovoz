using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Stock;

namespace Vodovoz.Controllers
{
	public class InventoryDocumentController
	{
		private readonly InventoryDocument _inventoryDocument;
		private readonly IInteractiveService _interactiveService;

		public InventoryDocumentController(
			InventoryDocument inventoryDocument,
			IInteractiveService interactiveService)
		{
			_inventoryDocument = inventoryDocument ?? throw new ArgumentNullException(nameof(inventoryDocument));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}
		
		public bool ConfirmInventoryDocument()
		{
			//TODO проверка на расхождения в экземплярном учете
			
			if(_inventoryDocument.InstanceItems.Any(x => !string.IsNullOrWhiteSpace(x.DiscrepancyDescription)))
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Невозможно подтвердить документ\n" +
					"Имеются расхождения в экземплярном учете");
				return false;
			}

			_inventoryDocument.InventoryDocumentStatus = InventoryDocumentStatus.Confirmed;
			return true;
		}
	}
}
