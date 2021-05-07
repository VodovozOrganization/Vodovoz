using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DriverAPI.Models
{
    public class ErrorResponseModel
    {
        public string Error { get; set; }

        public ErrorResponseModel(string message)
        {
            Error = message;
        }
    }
}
