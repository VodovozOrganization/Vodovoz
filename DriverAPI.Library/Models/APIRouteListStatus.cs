using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DriverAPI.Library.Models
{
    public enum APIRouteListStatus
    {
        New,
        Confirmed,
        InLoading,
        EnRoute,
        Delivered,
        OnClosing,
        MileageCheck,
        Closed
    }
}
