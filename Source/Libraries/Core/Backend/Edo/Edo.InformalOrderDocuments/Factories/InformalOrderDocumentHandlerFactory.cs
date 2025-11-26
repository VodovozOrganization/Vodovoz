using Edo.InformalOrderDocuments.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Orders;

namespace Edo.InformalOrderDocuments.Factories
{
	public class InformalOrderDocumentHandlerFactory : IInformalOrderDocumentHandlerFactory
	{
		private readonly Dictionary<OrderDocumentType, IInformalOrderDocumentHandler> _handlers;

		public InformalOrderDocumentHandlerFactory(IEnumerable<IInformalOrderDocumentHandler> handlers)
		{
			_handlers = handlers?.ToDictionary(h => h.DocumentType, h => h)
				?? throw new ArgumentNullException(nameof(handlers));
		}

		public IInformalOrderDocumentHandler GetHandler(OrderDocumentType documentType)
		{
			if(_handlers.TryGetValue(documentType, out var handler))
			{
				return handler;
			}

			throw new InvalidOperationException($"Обработчик для типа документа {documentType} не найден");
		}
	}
}
