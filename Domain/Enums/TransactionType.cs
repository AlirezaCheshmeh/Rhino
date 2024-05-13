using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    public enum TransactionType
    {
        [Display(Name ="پرداختی")]
        OutBound = 1,
        [Display(Name = "دریافتی")]
        InBound
    }
}
