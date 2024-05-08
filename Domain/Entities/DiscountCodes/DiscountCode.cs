using Domain.Entities.BaseEntity;
using Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.DiscountCodes
{
    public class DiscountCode : BaseEntity<long>
    {
        public string Code { get; private set; }
        public DateTime ExpireDate { get; private set; }
        public int UseCount { get; private set; }

        public void MinusUseCount() => UseCount--;
        public bool UseCountValidation() => UseCount > 0;
        public bool ExpireDateValidation() => ExpireDate > DateTime.Now;
        public static string GenerateDiscountCode(int length)
        {
            if (length > 26)
            {
                throw new ArgumentException("Length cannot be greater than the number of unique characters available.");
            }

            List<char> charPool = new List<char>();
            for (char c = 'a'; c <= 'z'; c++)
            {
                charPool.Add(c);
            }

            Random random = new Random();
            for (int i = charPool.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                char temp = charPool[i];
                charPool[i] = charPool[j];
                charPool[j] = temp;
            }
            return new string(charPool.GetRange(0, length).ToArray());
        }

    }
}
