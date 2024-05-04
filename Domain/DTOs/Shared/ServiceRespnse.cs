using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Domain.DTOs.Shared;

/// <summary>
/// for returning response from api
/// </summary>
/// <typeparam name="T"></typeparam> object you want return 
#region ServiceResponse With Data

public interface IServiceResponse<T>
{
    ServiceRespnse<T> OK(T data, int? total, HttpStatusCode status = HttpStatusCode.OK);
    ServiceRespnse<T> Failed(HttpStatusCode status, Hashtable errors);
}

public class ServiceRespnse<T> : IServiceResponse<T>
{
    public string Message { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public T Data { get; set; }
    public int? TotalCount { get; set; }
    public Hashtable Errors { get; set; }

    public ServiceRespnse<T> Failed(HttpStatusCode status, Hashtable errors)
    {
        return new ServiceRespnse<T>() { IsSuccess = false, Errors = errors, Message = "Operation Failed", StatusCode = StatusCode };
    }

    public ServiceRespnse<T> OK(T data, int? total = null, HttpStatusCode status = HttpStatusCode.OK)
    {
        return new ServiceRespnse<T>() { IsSuccess = true, Data = data, TotalCount = total };
    }
}

#endregion

#region ServiceResponse Without Data
public interface IServiceResponse
{
    ServiceRespnse OK(HttpStatusCode status = HttpStatusCode.OK);
    ServiceRespnse Failed(HttpStatusCode status, Hashtable errors);
}

public class ServiceRespnse : IServiceResponse
{
    public string Message { get; set; }
    public bool IsSuccess { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public Hashtable Errors { get; set; }

    public ServiceRespnse Failed(HttpStatusCode status, Hashtable errors)
    {
        return new ServiceRespnse() { IsSuccess = false, Errors = errors, Message = "Operation Failed", StatusCode = status };
    }

    public ServiceRespnse OK(HttpStatusCode status = HttpStatusCode.OK)
    {
        return new ServiceRespnse() { IsSuccess = true, Message = "Operation Success" };
    }
}
#endregion


#region PaymentResponses

public interface IPaymentZarinPalServiceResponse<T>
{
    PaymentZarinPalServiceResponse<T> OK(T data, HttpStatusCode status = HttpStatusCode.OK, string? Authority = null, string? RefId = null, string? Code = null);
    PaymentZarinPalServiceResponse<T> Failed(HttpStatusCode status, string errors);
}

public class PaymentZarinPalServiceResponse<T> : IPaymentZarinPalServiceResponse<T>
{
    public string ErrorMessage { get; set; }
    public HttpStatusCode Status { get; set; }
    public bool IsSuccess { get; set; }
    public string Pay_Authority { get; set; }
    public string Pay_RefId { get; set; }
    public string? Pay_Code { get; set; }
    public T Data { get; set; }

    public PaymentZarinPalServiceResponse<T> Failed(HttpStatusCode status, string errors)
    {
        return new PaymentZarinPalServiceResponse<T>() { ErrorMessage = errors, Status = status, IsSuccess = false };
    }

    public PaymentZarinPalServiceResponse<T> OK(T data, HttpStatusCode status = HttpStatusCode.OK, string? Authority = null, string? RefId = null, string? Code = null)
    {
        return new PaymentZarinPalServiceResponse<T>() { Data = data, Status = status, IsSuccess = true, Pay_Authority = Authority, Pay_RefId = Pay_RefId, Pay_Code = Code };
    }
}

#endregion

