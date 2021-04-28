using DriverAPI.Library.Models;
using System;

namespace DriverAPI.Controllers
{
    public class DriverActionModel
    {
        public APIActionType ActionType { get; set; }
        public DateTime ActionTime { get; set; }
    }
}
