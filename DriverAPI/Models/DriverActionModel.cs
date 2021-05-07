using DriverAPI.Library.Models;
using System;
using System.Text.Json.Serialization;

namespace DriverAPI.Controllers
{
    public class DriverActionModel
    {
        public APIActionType ActionType { get; set; }
        public DateTime ActionTime { get; set; }
    }
}
