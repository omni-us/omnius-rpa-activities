using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnius
{
    public static class StringExstensions
    {

        public static bool EqualsIgnoreCase(this string s, String other)
        {
            if (s == null && other == null)
            {
                return true;
            }

            if (s != null && other != null)
            {
                return s.ToLower().Equals(other.ToLower());
            }

            return false;
        }
    }
}
