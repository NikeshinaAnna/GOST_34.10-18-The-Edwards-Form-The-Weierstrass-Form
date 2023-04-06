using System;
using System.Collections.Generic;
using System.Text;

namespace GOST_34._10_12
{
    public static class Operations
    {
        public static BigInteger ExtendedEuclid(BigInteger base_n, BigInteger number)
        {
            var a = base_n;
            BigInteger res = 0;
            BigInteger w = 1;
            BigInteger temp;
            BigInteger temp1;
            BigInteger x;
            number = number + base_n;
            number = number % base_n;

            while (number > 0) // 
            {
                temp1 = a / number;
                x = number;
                number = a % x;
                a = x;
                x = w;
                temp = temp1 * x;
                w = res - temp;
                res = x;
            }
            res %= base_n;
            x = res + base_n;
            res = x % base_n;
            return res;
        }
    }
}
