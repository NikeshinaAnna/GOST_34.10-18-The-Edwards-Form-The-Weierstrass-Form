namespace GOST_34._10_12
{
    public class EdwardsCurvePoint : ICurve
    {
        public BigInteger e = new BigInteger("1", 10);
        public BigInteger d = new BigInteger("0605F6B7C183FA81578BC39CFAD518132B9DF62897009AF7E522C32D6DC7BFFB", 16);
        public BigInteger u;
        public BigInteger v;
        public BigInteger fieldChar;


        public EdwardsCurvePoint() { }
        public EdwardsCurvePoint(EllipticCurvePoint p)
        {
            this.fieldChar = p.fieldCharacteristic;
            var s = (e - d + fieldChar)* Operations.ExtendedEuclid(fieldChar, 4) % fieldChar;
            var t = (e + d) * Operations.ExtendedEuclid(fieldChar, 6) % fieldChar;
            var y_inverse = Operations.ExtendedEuclid(fieldChar, p.y);
            var v_inverse = Operations.ExtendedEuclid(fieldChar, p.x - t + s);
            u = (p.x - t + fieldChar) * y_inverse % fieldChar;
            v = (p.x - t - s + fieldChar) * v_inverse % fieldChar;
        }

        public EdwardsCurvePoint(BigInteger p, BigInteger u, BigInteger v)
        {
            fieldChar = p;
            this.u = u;
            this.v = v;
        }

        //public static EdwardsCurvePoint MultipluEdwardsPoint(BigInteger k, EdwardsCurvePoint point)
        //{
        //    var proectivePoint = new EdwardsCurveProectivePoint(point);
        //    return EdwardsCurveProectivePoint.getAffineEdwardsPoint(EdwardsCurveProectivePoint.multiplyEdwardsProectivePoint(proectivePoint, k));
        //}

        //public static EdwardsCurvePoint SumEdwardsPoint(EdwardsCurvePoint A, EdwardsCurvePoint B)
        //{
        //    var proectivePointA = new EdwardsCurveProectivePoint(A);
        //    var proectivePointB = new EdwardsCurveProectivePoint(B);
        //    return EdwardsCurveProectivePoint.getAffineEdwardsPoint(EdwardsCurveProectivePoint.AdditionPoints(proectivePointA, proectivePointB));
        //}

        public ICurve MultiplyPointByNumber(BigInteger number)
        {
            var proectivePoint = new EdwardsCurveProectivePoint(this);
            return EdwardsCurveProectivePoint.GetAffineEdwardsPoint(EdwardsCurveProectivePoint.MultiplyEdwardsProectivePoint(proectivePoint, number));
        }

        public ICurve AddPoints(ICurve point2)
        {
            var proectivePointA = new EdwardsCurveProectivePoint(this);
            var proectivePointB = new EdwardsCurveProectivePoint(point2 as EdwardsCurvePoint);
            return EdwardsCurveProectivePoint.GetAffineEdwardsPoint(EdwardsCurveProectivePoint.AdditionPoints(proectivePointA, proectivePointB));
        }
    }

    public class EdwardsCurveProectivePoint
    {
        public BigInteger u { get; set; }
        public BigInteger v { get; set; }
        public BigInteger z { get; set; }
        public BigInteger p { get; set; }
        public BigInteger e { get; set; }
        public BigInteger d { get; set; }

        public EdwardsCurveProectivePoint(EdwardsCurvePoint edwardsPoint)
        {
            u = edwardsPoint.u;
            v = edwardsPoint.v;
            z = 1;
            p = edwardsPoint.fieldChar;
            e = edwardsPoint.e;
            d = edwardsPoint.d;
        }

        public EdwardsCurveProectivePoint(BigInteger u, BigInteger v, BigInteger z, BigInteger p, BigInteger e, BigInteger d)
        {
            this.u = u;
            this.v = v;
            this.z = z;
            this.p = p;
            this.e = e;
            this.d = d;
        }

        public static EdwardsCurveProectivePoint MultiplyEdwardsProectivePoint(EdwardsCurveProectivePoint A, BigInteger k)
        {
            var res = new EdwardsCurveProectivePoint(0, 1, 1, A.p, A.e, A.d); ;
            var tmp = A;
            while (k != 0)
            {
                if (k % 2 == 1) //В этом месте основное отличие кривых в форме Эдвардса от кривых Вейерштраса: 
                                //здесь алгоритм сложения одинаковых точек ничем не отличается от алгоритма сложения различных точек.
                {
                    res = AdditionPoints(tmp, res);
                }
                tmp = AdditionPoints(tmp, tmp);
                k = k / 2;
            }
            return res;
        }

        public static EdwardsCurveProectivePoint AdditionPoints(EdwardsCurveProectivePoint P1, EdwardsCurveProectivePoint P2)
        {
            var p = P1.p;
            var A = P1.z * P2.z % p;
            var B = A * A % p;
            var C = P1.u * P2.u % p;
            var D = P1.v * P2.v % p;
            var E = P1.d * C * D % p;
            var F = (B - E + p) % p;
            var G = (B + E) % p;
            var H = (P1.u + P1.v) * (P2.u + P2.v) % p;

            var X3 = A * F * (H + 2*p - C - D) % p;
            var Y3 = A * G * (D - C + p) % p;
            var Z3 = F * G % p;

            return new EdwardsCurveProectivePoint(X3, Y3, Z3, p, P1.e, P1.d);
        }

        private static EdwardsCurveProectivePoint DoublePoint(EdwardsCurveProectivePoint P)
        {
            var p = P.p;
            var A = P.z * P.z % p;
            var B = A * A % p;
            var C = P.u * P.u % p;
            var D = P.v * P.v % p;
            var E = P.d * C * D % p;
            var F = (B - E + p) % p;
            var G = (B + E) % p;
            var H = (P.u + P.v) * (P.u + P.v) % p;

            var X3 = A * F * (H + 2 * p - C - D) % p;
            var Y3 = A * G * (D - C + p) % p;
            var Z3 = F * G % p;

            return new EdwardsCurveProectivePoint(X3, Y3, Z3, p, P.e, P.d);
        }


        public static EdwardsCurvePoint GetAffineEdwardsPoint(EdwardsCurveProectivePoint point)
        {
            var p = point.p;
            var z_inverse = Operations.ExtendedEuclid(p, point.z);
            var x = point.u * z_inverse % p;
            var y = point.v * z_inverse % p;

            return new EdwardsCurvePoint(p, x, y);
        }
    }
}
