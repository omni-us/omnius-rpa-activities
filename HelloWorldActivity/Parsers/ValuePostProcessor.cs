using Omnius;
using Omnius;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UiPath.SmartData.DataContracts.Taxonomy;

namespace Omnius
{
    public class ValuePostProcessor
    {
        private static Dictionary<FieldType, Func<String, String>> converters;

        static ValuePostProcessor()
        {
            converters = new Dictionary<FieldType, Func<string, string>>();
            converters[FieldType.Boolean] = Checkbox;
            converters[FieldType.Number] = Number;
            converters[FieldType.Text] = Text;

        }

        public Func<String, String> PostprocessFor(Field field)
        {
            var FieldSpecificConverter = converters.GetOrDefault(field.Type, FunctionExstensions.Identity);

            return ((Func<string, string>)NonNull)
                    .Then(HtmlDecode)
                    .Then(SwapCustomTags)
                    .Then(FieldSpecificConverter)
                    .Then(NonNull);
        }

        private static string SwapCustomTags(string s)
        {
            return s.Replace("<empty/>", "");
        }

        private static String Number(String s)
        {
            Regex regex = new Regex("^\\D");
            var res = s.Replace("l", "1").Replace("I", "1").Replace("/", "1").Replace("\\", "1").Replace("L", "1").Replace("O", "0").Replace("o", "0");
            return regex.Replace(res, "");
        }


        private static String Checkbox(String s)
        {
            if (s.IsNullOrWhiteSpace())
            {
                return "No";
            }

            return "Yes";
        }

        private static String Text(String s)
        {
            return s.Replace("0", "O");
        }

        private static String HtmlDecode(String s) => WebUtility.HtmlDecode(s);

        private static String NonNull(String s) => s ?? "";
    }
}
