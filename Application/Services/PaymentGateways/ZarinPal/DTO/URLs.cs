using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PaymentGateways.ZarinPal.DTO
{

    public class URLs
    {
        public const string gateWayUrl = "https://www.zarinpal.com/pg/StartPay/";
        public const string requestUrl = "https://api.zarinpal.com/pg/v4/payment/request.json";
        public const string verifyUrl = "https://api.zarinpal.com/pg/v4/payment/verify.json";

        //Test
        public const string SandgateWayUrl = "https://sandbox.zarinpal.com/pg/StartPay/";
        public const string SandrequestUrl = "https://sandbox.zarinpal.com/pg/v4/payment/request.json";
        public const string SandverifyUrl = "https://sandbox.zarinpal.com/pg/v4/payment/verify.json";
    }
}
