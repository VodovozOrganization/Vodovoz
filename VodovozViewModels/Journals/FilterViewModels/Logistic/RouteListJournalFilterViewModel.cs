using QS.Project.Filter;
using QS.Project.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
    public class RouteListJournalFilterViewModel : FilterViewModelBase<RouteListJournalFilterViewModel>
    {
        public RouteListJournalFilterViewModel()
        {
            foreach(var status in Enum.GetValues(typeof(RouteListStatus)).Cast<RouteListStatus>())
            {
                statusNodes.Add(new RouteListStatusNode(status));
            }

            foreach (var addressType in Enum.GetValues(typeof(AddressType)).Cast<AddressType>())
            {
                addressTypeNodes.Add(new AddressTypeNode(addressType));
            }

            GeographicGroups = UoW.Session.QueryOver<GeographicGroup>().List<GeographicGroup>().ToList();

            var currentUserSettings = UserSingletonRepository.GetInstance().GetUserSettings(UoW, ServicesConfig.CommonServices.UserService.CurrentUserId);
            foreach (var addressTypeNode in AddressTypeNodes)
            {
                switch (addressTypeNode.AddressType)
                {
                    case AddressType.Delivery:
                        addressTypeNode.Selected = currentUserSettings.LogisticDeliveryOrders;
                        break;
                    case AddressType.Service:
                        addressTypeNode.Selected = currentUserSettings.LogisticServiceOrders;
                        break;
                    case AddressType.ChainStore:
                        addressTypeNode.Selected = currentUserSettings.LogisticChainStoreOrders;
                        break;
                }
            }
        }

        private DeliveryShift deliveryShift;
        /// <summary>
        /// Смена доставки
        /// </summary>
        public DeliveryShift DeliveryShift
        {
            get { return deliveryShift; }
            set
            {
                UpdateFilterField(ref deliveryShift, value);
            }
        }

        private DateTime? startDate;
        /// <summary>
        /// Дата начала среза выборки по дате
        /// </summary>
        public DateTime? StartDate
        {
            get { return startDate; }
            set
            {
                UpdateFilterField(ref startDate, value);
            }
        }

        private DateTime? endDate;
        /// <summary>
        /// Дата окончания среза выборки по дате
        /// </summary>
        public DateTime? EndDate
        {
            get { return endDate; }
            set
            {
                UpdateFilterField(ref endDate, value);
            }
        }

        private List<GeographicGroup> geographicGroups;
        /// <summary>
        /// Части города для отображения в фильтре
        /// </summary>
        public List<GeographicGroup> GeographicGroups { get => geographicGroups; set => geographicGroups = value; }


        private GeographicGroup geographicGroup;
        /// <summary>
        /// Часть города
        /// </summary>
        public GeographicGroup GeographicGroup
        {
            get { return geographicGroup; }
            set
            {
                UpdateFilterField(ref geographicGroup, value);
            }
        }

        #region RouteListStatus
        /// <summary>
        /// Отображаемые в фильтре статусы
        /// </summary>
        public RouteListStatus[] DisplayableStatuses
        {
            get { return StatusNodes.Select(rn => rn.RouteListStatus).ToArray(); }
            set
            {
                foreach(var status in value)
                {
                    if (!statusNodes.Any(sn => sn.RouteListStatus == status))
                    {
                        statusNodes.Add(new RouteListStatusNode(status));
                    }
                }

                statusNodes.RemoveAll(rn => !value.Contains(rn.RouteListStatus));

                OnPropertyChanged(() => DisplayableStatuses);
                OnPropertyChanged(() => StatusNodes);
            }
        }

        /// <summary>
        /// Статусы в фильтре предустановлены и не могут изменяться, если в поле есть хотя бы 1 статус
        /// </summary>
        public RouteListStatus[] RestrictedByStatuses
        {
            get
            {
                if (CanSelectStatuses)
                {
                    return new RouteListStatus[] { };
                } else
                {
                    return SelectedStatuses;
                }
            }
            set
            {
                if (value.Any())
                {
                    DisplayableStatuses = value;
                    SelectedStatuses = value;
                    CanSelectStatuses = false;
                } else
                {
                    CanSelectStatuses = true;
                }
            }
        }

        /// <summary>
        /// Статусы в фильтре предустановлены и не могут изменяться, если в поле есть хотя бы 1 статус
        /// </summary>
        public RouteListStatus[] SelectedStatuses
        {
            get
            {
                return StatusNodes.Where(rn => rn.Selected)
                    .Select(rn => rn.RouteListStatus).ToArray();
            }
            set
            {
                foreach (var status in statusNodes.Where(rn => value.Contains(rn.RouteListStatus)))
                {
                    status.Selected = true;
                }

                OnPropertyChanged(() => StatusNodes);
            }
        }

        List<RouteListStatusNode> statusNodes = new List<RouteListStatusNode>();
        /// <summary>
        /// Строки статусов таблице с чекбоксами
        /// </summary>
        public List<RouteListStatusNode> StatusNodes {
            get { return statusNodes; } 
            private set
            {
                UpdateFilterField(ref statusNodes, value);
            }
        }
        #endregion

        private List<AddressTypeNode> addressTypeNodes = new List<AddressTypeNode>();
        /// <summary>
        /// Типы выездов
        /// </summary>
        public List<AddressTypeNode> AddressTypeNodes 
        {
            get => addressTypeNodes; 
            set => addressTypeNodes = value; 
        }

        private RLFilterTransport? transportType;
        /// <summary>
        /// Тип транспорта для доставки
        /// </summary>
        public RLFilterTransport? TransportType
        {
            get { return transportType; }
            set
            {
                UpdateFilterField(ref transportType, value);
            }
        }

        public bool WithDeliveryAddresses => AddressTypeNodes.Any(an => an.AddressType == AddressType.Delivery && an.Selected);

        public bool WithServiceAddresses => AddressTypeNodes.Any(an => an.AddressType == AddressType.Service && an.Selected);

        public bool WithChainStoreAddresses => AddressTypeNodes.Any(an => an.AddressType == AddressType.ChainStore && an.Selected);

        public bool CanSelectStatuses { get; private set; }

        /// <summary>
        /// Типы транспорта для доставки
        /// </summary>
        public enum RLFilterTransport
        {
            [Display(Name = "Наёмники")]
            Mercenaries,
            [Display(Name = "Раскат")]
            Raskat,
            [Display(Name = "Ларгус")]
            Largus,
            [Display(Name = "ГАЗель")]
            GAZelle,
            [Display(Name = "Фура")]
            Waggon,
            [Display(Name = "Прочее")]
            Others
        }
    }
}
