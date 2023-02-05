using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.Controllers
{
	public class RouteListUnderLoadDocumentController: IRouteListUnderLoadDocumentController
	{
		private readonly IEmployeeRepository _employeeRepository;

		public RouteListUnderLoadDocumentController(IEmployeeRepository employeeRepository)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
		}

		public void CreateOrUpdateCarUnderloadDocument(IUnitOfWork uow, RouteList routeList)
		{
			var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(uow);

			var carUnderloadDocument =
				uow.GetAll<CarUnderloadDocument>()
					.SingleOrDefault(x => x.RouteList.Id == routeList.Id)
				?? new CarUnderloadDocument();

			carUnderloadDocument.RouteList = routeList;
			carUnderloadDocument.Author = currentEmployee;

			carUnderloadDocument.Items.Clear();

			var notLoadedItems = routeList.NotLoadedNomenclatures(false);

			CreateUnderloadDocumentItems(notLoadedItems, carUnderloadDocument);

			if(carUnderloadDocument.Items.Any())
			{
				uow.Save(carUnderloadDocument);
			}
			else
			{
				uow.Delete(carUnderloadDocument);
			}
		}

		private void CreateUnderloadDocumentItems(List<RouteListControlNotLoadedNode> notLoadedItems, CarUnderloadDocument carUnderloadDocument)
		{
			foreach(var notLoadedItem in notLoadedItems)
			{
				if(notLoadedItem.CountNotLoaded == 0)
				{
					continue;
				}

				var underloadItem = carUnderloadDocument.Items.SingleOrDefault(x => x.Nomenclature.Id == notLoadedItem.Nomenclature.Id)
				                    ?? new CarUnderloadDocumentItem();

				underloadItem.Amount = -notLoadedItem.CountNotLoaded;
				underloadItem.Nomenclature = notLoadedItem.Nomenclature;
				underloadItem.CarUnderloadDocument = carUnderloadDocument;

				underloadItem.CreateOrUpdateOperation();

				carUnderloadDocument.Items.Add(underloadItem);
			}
		}
	}
}
