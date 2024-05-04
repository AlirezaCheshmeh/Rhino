using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extensions
{
    public static class DictionaryExtension
    {
        internal static string ToJson(Dictionary<string, string> validations)
        {
            return JsonConvert.SerializeObject(validations);
        }
    }
}
