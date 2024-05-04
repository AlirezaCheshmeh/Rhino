using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PaymentGateways.ZarinPal.DTO
{
    public class VerifyParameters
    {
        public string amount { set; get; }
        public string merchant_id { set; get; }
        public string authority { set; get; }
    }
}
