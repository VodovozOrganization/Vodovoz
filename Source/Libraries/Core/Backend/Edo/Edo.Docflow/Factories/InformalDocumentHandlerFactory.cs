using Edo.Docflow.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Orders;

namespace Edo.Docflow.Factories
{
	public class InformalDocumentHandlerFactory : IInformalOrderDocumentHandlerFactory
	{

		private readonly Dictionary<OrderDocumentType, IInformalOrderDocumentHandler> _handlers;

		public InformalDocumentHandlerFactory(IEnumerable<IInformalOrderDocumentHandler> handlers)
		{
			_handlers = handlers.ToDictionary(h => h.DocumentType, h => h);
		}

		public IInformalOrderDocumentHandler GetHandler(OrderDocumentType documentType) => _handlers.TryGetValue(documentType, out var handler)
			? handler
			: throw new NotSupportedException($"Обработчик для типа документа {documentType} не найден");
	}
}
