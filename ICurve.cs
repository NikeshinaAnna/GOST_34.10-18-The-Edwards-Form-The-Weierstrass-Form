using System;
using System.Collections.Generic;
using System.Text;

namespace GOST_34._10_12
{
    public interface ICurve
    {
        ICurve MultiplyPointByNumber(BigInteger number);
        ICurve AddPoints(ICurve point2);
    }
}
