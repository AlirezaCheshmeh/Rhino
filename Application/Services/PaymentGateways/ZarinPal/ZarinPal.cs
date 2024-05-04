using Application.Services.PaymentGateways.ZarinPal.DTO;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs.Shared;

namespace Application.Services.PaymentGateways.ZarinPal
{
    public static class ZarinPal
    {
        const string merchant = "your-merchant-string";
        const string description = "your description";
        const string Callbackurl = "your comeback url";

        public async static Task<IPaymentZarinPalServiceResponse<string>> PayRequest(string amount, string? phone,
            string callbackurl = Callbackurl)
        {
            try
            {
                DTO.RequestParameters Parameters =
                    new Application.Services.PaymentGateways.ZarinPal.DTO.RequestParameters(merchant, amount,
                        description, callbackurl, phone, "");

                var client1 = new RestClient(URLs.requestUrl);

                Method method = Method.Post;

                var request = new RestRequest("", method);

                request.AddHeader("accept", "application/json");

                request.AddHeader("content-type", "application/json");

                request.AddJsonBody(Parameters);

                var requestresponse = await client1.ExecuteAsync(request);

                JObject jo = JObject.Parse(requestresponse.Content);

                string errorscode = jo["errors"].ToString();

                JObject jodata = JObject.Parse(requestresponse.Content);

                string dataauth = jodata["data"].ToString();


                if (dataauth != "[]" && errorscode == "[]")
                {
                    var authority = jodata["data"]["authority"].ToString();

                    string gatewayUrl = URLs.gateWayUrl + authority;
                    return new PaymentZarinPalServiceResponse<string>
                    {
                        Data = gatewayUrl,
                        IsSuccess = true,
                        Pay_Authority = authority,
                    };
                }
                else
                {
                    return new PaymentZarinPalServiceResponse<string>
                    {
                        Data = null,

                        IsSuccess = false,
                        ErrorMessage = $"PayError:{errorscode}"
                    };
                }
            }

            catch (Exception ex)
            {
                return new PaymentZarinPalServiceResponse<string>
                {
                    Data = null,
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public static IPaymentZarinPalServiceResponse<string> VerifyPayment(string Authority, string amount,
            string Status)
        {
            if (Status == "OK")
            {
                VerifyParameters parameters = new VerifyParameters();

                parameters.authority = Authority;
                parameters.amount = amount;
                parameters.merchant_id = merchant;


                var client = new RestClient(URLs.verifyUrl);
                Method method = Method.Post;
                var request = new RestRequest("", method);

                request.AddHeader("accept", "application/json");
                request.AddHeader("content-type", "application/json");
                request.AddJsonBody(parameters);

                var response = client.ExecuteAsync(request);
                JObject jodata = JObject.Parse(response.Result.Content);
                string data = jodata["data"].ToString();
                JObject jo = JObject.Parse(response.Result.Content);
                string errors = jo["errors"].ToString();

                if (data != "[]")
                {
                    string refid = jodata["data"]["ref_id"].ToString();
                    string code = jodata["data"]["code"].ToString();
                    if (code == "100" || code == "101")
                    {
                        //add something to database                   
                        Status = "OK";
                        return new PaymentZarinPalServiceResponse<string>
                        {
                            Data = "پرداخت با موفقیت انجام شد",
                            Pay_Code = code,
                            Pay_RefId = refid,
                            IsSuccess = true,
                        };
                    }
                    else
                    {
                        return new PaymentZarinPalServiceResponse<string>
                        {
                            Data = "ناموفق بود ",
                            IsSuccess = false,
                        };
                    }
                }
                else if (errors != "[]")
                {
                    string errorscode = jo["errors"]["code"].ToString();
                    Status = "NOK";
                    return new PaymentZarinPalServiceResponse<string>
                    {
                        IsSuccess = false,
                        ErrorMessage = errorscode,
                        Data = "ناموفق بود ",
                        Pay_Code = jodata["data"]["code"].ToString()
                    };
                }
                else
                {
                    return new PaymentZarinPalServiceResponse<string>
                    {
                        Data = "ناموفق بود ",
                        IsSuccess = false,
                    };
                }
            }
            else
            {
                return new PaymentZarinPalServiceResponse<string>
                {
                    Data = "ناموفق بود ",
                    IsSuccess = false,
                };
            }
        }
    }
}