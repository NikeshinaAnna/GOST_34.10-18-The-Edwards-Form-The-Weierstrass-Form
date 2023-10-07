using GOST_34._10_18;
using System;
using System.Collections.Generic;
using System.Text;

namespace GOST_34._10_12
{
    class WeierstrassCurveDecartPoint
    {
        public BigInteger x { get; set; }
        public BigInteger y { get; set; }
        public BigInteger p { get; set; }
        public BigInteger a { get; set; }
        public BigInteger b { get; set; }

        public WeierstrassCurveDecartPoint(BigInteger p_x, BigInteger p_y, BigInteger p_p, BigInteger p_a, BigInteger p_b)
        {
            x = p_x;
            y = p_y;
            p = p_p;
            a = p_a;
            b = p_b;
        }

        public WeierstrassCurveDecartPoint(EllipticCurvePoint ellipticPoint)
        {
            x = ellipticPoint.x;
            y = ellipticPoint.y;
            p = ellipticPoint.fieldCharacteristic;
            a = ellipticPoint.a;
            b = ellipticPoint.b;
        }

        public WeierstrassCurveDecartPoint AdditionPoints(WeierstrassCurveDecartPoint point1, WeierstrassCurveDecartPoint point2)
        {
            if (point1.x == 0 && point1.y == 0)
                return point2;
            if (point2.x == 0 && point2.y == 0)
                return point1;
            BigInteger x_diff = point1.x - point2.x;
            BigInteger y_diff = point1.y - point2.y;
            if (x_diff < 0)
                x_diff += p;
            if (y_diff < 0)
                y_diff += p;

            BigInteger m = y_diff * Operations.ExtendedEuclid(p, x_diff)%p;
            (BigInteger x, BigInteger y) = getCoordinates(p, point1.x, point1.y, point2.x, m);

            return new WeierstrassCurveDecartPoint(x, y, p, a, b);
        }

        public WeierstrassCurveDecartPoint DoublePoint(WeierstrassCurveDecartPoint point)
        {
            BigInteger y_rev = Operations.ExtendedEuclid(p, 2 * point.y % p);
            BigInteger m = (3 * point.x * point.x % p + point.a)*y_rev %p;
            (BigInteger x, BigInteger y) = getCoordinates(p, point.x, point.y, point.x, m);
            return new WeierstrassCurveDecartPoint(x, y, p, a, b);
        }

        public WeierstrassCurveDecartPoint MultiplyPoint(WeierstrassCurveDecartPoint point, BigInteger N)
        {
            WeierstrassCurveDecartPoint result = new WeierstrassCurveDecartPoint(0, 0, point.p, point.a, point.b);
            var tmp = point;
            while (N != 0)
            {
                if (N % 2 == 1)
                {
                    result = AdditionPoints(tmp, result);
                }
                tmp = DoublePoint(tmp);
                N = N / 2;
            }
            return result;
        }

        private (BigInteger, BigInteger) getCoordinates (BigInteger p, BigInteger x_p, BigInteger y_p, BigInteger x_q, BigInteger m)
        {
            BigInteger x = (m * m - x_p - x_q) % p;
            if (x < 0)
                x += p;

            BigInteger x_diff = (x_p - x + p) % p;
            BigInteger y = (m * x_diff - y_p + p) % p;
            return (x,y);
        }
    }
}
