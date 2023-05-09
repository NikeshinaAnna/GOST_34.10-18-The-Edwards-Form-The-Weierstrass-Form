using System;
using System.Collections.Generic;
using System.Text;

namespace GOST_34._10_18
{
    public interface ICurve
    {
        ICurve MultiplyPointByNumber(BigInteger number);
        ICurve AddPoints(ICurve point2);
    }
}
