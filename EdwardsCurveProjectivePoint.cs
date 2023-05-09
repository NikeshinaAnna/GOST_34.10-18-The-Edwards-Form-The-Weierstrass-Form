namespace GOST_34._10_18
{
    class EdwardsCurveProjectivePoint
    {
        public BigInteger u { get; set; }
        public BigInteger v { get; set; }
        public BigInteger z { get; set; }
        public BigInteger p { get; set; }
        public BigInteger e { get; set; }
        public BigInteger d { get; set; }

        public EdwardsCurveProjectivePoint(EdwardsCurvePoint edwardsPoint)
        {
            u = edwardsPoint.u;
            v = edwardsPoint.v;
            z = 1;
            p = edwardsPoint.fieldChar;
            e = edwardsPoint.e;
            d = edwardsPoint.d;
        }

        public EdwardsCurveProjectivePoint(BigInteger u, BigInteger v, BigInteger z, BigInteger p, BigInteger e, BigInteger d)
        {
            this.u = u;
            this.v = v;
            this.z = z;
            this.p = p;
            this.e = e;
            this.d = d;
        }

        public static EdwardsCurveProjectivePoint MultiplyEdwardsProectivePoint(EdwardsCurveProjectivePoint A, BigInteger k)
        {
            var res = new EdwardsCurveProjectivePoint(0, 1, 1, A.p, A.e, A.d); ;
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

        public static EdwardsCurveProjectivePoint AdditionPoints(EdwardsCurveProjectivePoint P1, EdwardsCurveProjectivePoint P2)
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

            var X3 = A * F * (H + 2 * p - C - D) % p;
            var Y3 = A * G * (D - C + p) % p;
            var Z3 = F * G % p;

            return new EdwardsCurveProjectivePoint(X3, Y3, Z3, p, P1.e, P1.d);
        }

        public static EdwardsCurvePoint GetAffineEdwardsPoint(EdwardsCurveProjectivePoint point)
        {
            var p = point.p;
            var z_inverse = Operations.ExtendedEuclid(p, point.z);
            var x = point.u * z_inverse % p;
            var y = point.v * z_inverse % p;

            return new EdwardsCurvePoint(p, x, y);
        }
    }
}
