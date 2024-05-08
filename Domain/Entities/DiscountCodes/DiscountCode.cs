using Domain.Entities.BaseEntity;
using Domain.Entities.Plans;
using Domain.Entities.UserPurchases;

namespace Domain.Entities.DiscountCodes
{
    public class DiscountCode : BaseEntity<long>
    {
        public string Code { get; private set; }
        public DateTime ExpireDate { get; private set; }
        public int UseCount { get; private set; }
        public long PlanId { get; set; }
        public Plan Plan { get; set; }
        public ICollection<UserPurchase> UserPurchases { get; set; }

        public void MinusUseCount() => UseCount--;
        public bool UseCountValidation() => UseCount > 0;
        public bool ExpireDateValidation() => ExpireDate > DateTime.Now;
        public static string GenerateDiscountCode(int length)
        {
            if (length > 26)
            {
                throw new ArgumentException("Length cannot be greater than the number of unique characters available.");
            }

            List<char> charList = new List<char>();
            for (char c = 'a'; c <= 'z'; c++)
            {
                charList.Add(c);
            }

            Random random = new Random();
            for (int i = charList.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                char temp = charList[i];
                charList[i] = charList[j];
                charList[j] = temp;
            }
            return new string(charList.GetRange(0, length).ToArray());
        }

        public static DiscountCode CreateDiscountCode(DateTime expireDate, int useCount, long planId,int? codeLength = null) => new()
        {
            //fedault is 5 length
            Code = GenerateDiscountCode(codeLength.HasValue?codeLength.Value:5),
            ExpireDate = expireDate,
            PlanId = planId,
            UseCount = useCount,
        };
    }
}
