using System;

namespace VodovozMangoService.DTO
{
    public class EventRequest
    {
        public string Vpbx_Api_Key { get; set; }

        public string Sign{ get; set; }

        public string Json { get; set; }
    }
}