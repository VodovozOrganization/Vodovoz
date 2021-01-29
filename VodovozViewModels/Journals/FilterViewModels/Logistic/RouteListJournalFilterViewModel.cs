using QS.Project.Filter;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
    public class RouteListJournalFilterViewModel : FilterViewModelBase<RouteListJournalFilterViewModel>
    {
        public RouteListJournalFilterViewModel()
        {

        }
        
        private RouteListStatus[] restrictedByStatuses;
        /// <summary>
        /// Статусы в фильтре ограниченны указанными в поле, если есть хоть 1
        /// </summary>
        public RouteListStatus[] RestrictedByStatuses
        {
            get { return restrictedByStatuses; }
            set
            {
                UpdateFilterField(ref restrictedByStatuses, value);
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

        RouteListStatus[] displayableStatuses = Enum.GetValues(typeof(RouteListStatus)).Cast<RouteListStatus>().ToArray();
        /// <summary>
        /// Отображаемые в фильтре статусы
        /// </summary>
        public RouteListStatus[] DisplayableStatuses
        {
            get { return displayableStatuses; }
            set
            {
                UpdateFilterField(ref displayableStatuses, value);
            }
        }

        RouteListStatus[] statuses;
        /// <summary>
        /// Статусы для фильтрации
        /// </summary>
        public RouteListStatus[] Statuses
        {
            get { return statuses; }
            set
            {
                UpdateFilterField(ref statuses, value);
            }
        }
        
        private AddressType[] addressTypes;
        /// <summary>
        /// Типы выездов
        /// </summary>
        public AddressType[] AddressTypes
        {
            get { return addressTypes; }
            set
            {
                UpdateFilterField(ref addressTypes, value);
            }
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

        public bool WithDeliveryAddresses => AddressTypes.Contains(AddressType.Delivery);

        public bool WithServiceAddresses => AddressTypes.Contains(AddressType.Service);

        public bool WithChainStoreAddresses => AddressTypes.Contains(AddressType.ChainStore);

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
