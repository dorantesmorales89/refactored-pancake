using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PruebaIngreso.Models
{
    public class ApiResult
    {
        public string Code { get; set; }
        public double Margin { get; set; }
        public int StatusCode { get; set; }
    }
}