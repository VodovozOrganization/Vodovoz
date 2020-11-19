using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Services;

namespace Vodovoz.Controllers
{
    public class DeliveryDocumentController : IDeliveryDocumentController
    {
        public DeliveryDocumentController(IStandartNomenclatures standartNomenclaturesParameters, IEmployeeRepository employeeRepository)
        {
            this.standartNomenclaturesParameters = standartNomenclaturesParameters ?? throw new ArgumentNullException(nameof(standartNomenclaturesParameters));
            this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        }

        private readonly IStandartNomenclatures standartNomenclaturesParameters;
        private readonly IEmployeeRepository employeeRepository;
        
        public void UpdateDocuments(RouteList routeList, IUnitOfWork uow)
        {
            switch (routeList.Status) {
                case RouteListStatus.OnClosing:
                    DeleteDeliveryDocuments(routeList, uow);
                    break;
                case RouteListStatus.Closed:
                    CreateOrUpdateDeliveryDocuments(uow, routeList);
                    break;
                default:
                    return;
            }
        }

        private void DeleteDeliveryDocuments(RouteList routeList, IUnitOfWork uow)
        {
            var documents = uow.Session.QueryOver<DeliveryDocument>()
                .WhereRestrictionOn(x => x.RouteListItem.Id)
                .IsIn(routeList.Addresses.Select(x => x.Id).ToArray())
                .List();

            foreach (var document in documents) {
                uow.Delete(document);
            }
        }
        
        private void CreateOrUpdateDeliveryDocuments(IUnitOfWork uow, RouteList routeList)
        {
            var deliveryDocuments = uow.Session.QueryOver<DeliveryDocument>()
                .WhereRestrictionOn(x => x.RouteListItem.Id)
                .IsIn(routeList.Addresses.Select(x => x.Id).ToArray())
                .List();
            
            var employeeForCurrentUser = employeeRepository.GetEmployeeForCurrentUser(uow);

            foreach (var address in routeList.Addresses.Where(x => x.Status == RouteListItemStatus.Completed)) {
                var deliveryDocument = deliveryDocuments.FirstOrDefault(x => x.RouteListItem.Id == address.Id) ?? new DeliveryDocument();

                if(deliveryDocument.Author == null)
                    deliveryDocument.Author = employeeForCurrentUser;
                    
                if(deliveryDocument.TimeStamp == default(DateTime))
                    deliveryDocument.TimeStamp = DateTime.Now;
                
                deliveryDocument.LastEditor = employeeForCurrentUser;
                deliveryDocument.LastEditedTime = DateTime.Now;
                deliveryDocument.RouteListItem = address;
				
                deliveryDocument.UpdateItems(address, uow.GetById<Nomenclature>(standartNomenclaturesParameters.GetReturnedBottleNomenclatureId));
                uow.Save(deliveryDocument);
            }
        }
    }
}