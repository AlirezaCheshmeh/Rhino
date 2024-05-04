using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PaymentGateways.ZarinPal.DTO
{
    public class RequestParameters
    {
        public string merchant_id { get; set; }

        public string amount { get; set; }
        public string description { get; set; }
        public string callback_url { get; set; }

        public metadata? metadata { get; set; }

        public RequestParameters(string merchant_id, string amount, string description, string callback_url, string? mobile, string? email)
        {
            this.merchant_id = merchant_id;
            this.amount = amount;
            this.description = description;
            this.callback_url = callback_url;
            metadata = new metadata();
            if (mobile != null)
            {
                metadata.mobile = mobile;
            }
            if (email != null)
            {
                metadata.email = email;
            }


        }
    }
    public class metadata
    {
        public string? mobile { get; set; }
        public string? email { get; set; }
    }
}
