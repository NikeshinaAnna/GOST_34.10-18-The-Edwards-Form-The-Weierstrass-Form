using GOST_34._10_18;

namespace GOST_34._10_12
{
    class EdwardsCurveDecartPoint
    {
        public BigInteger u { get; set; }
        public BigInteger v { get; set; }
        public BigInteger p { get; set; }
        public BigInteger e { get; set; }
        public BigInteger d { get; set; }

        public EdwardsCurveDecartPoint(BigInteger p_u, BigInteger p_v, BigInteger p_p, BigInteger p_e, BigInteger p_d)
        {
            u = p_u;
            v = p_v;
            p = p_p;
            e = p_e;
            d = p_d;
        }

        public EdwardsCurveDecartPoint(EdwardsCurvePoint edwElPoint)
        {
            u = edwElPoint.u;
            v = edwElPoint.v;
            p = edwElPoint.fieldChar;
            e = edwElPoint.e;
            d = edwElPoint.d;
        }

        public EdwardsCurveDecartPoint AdditionPoints(EdwardsCurveDecartPoint p, EdwardsCurveDecartPoint q)
        {
            if (p.u == 0 && p.v == 1)
                return q;
            if (q.u == 0 && q.v == 1)
                return p;
            BigInteger multiCoord = (p.d * p.u * p.v * q.v * q.u) % p.p;
            while (multiCoord < 0)
                multiCoord += p.p;
            BigInteger u_numerator = (p.u * q.v + p.v * q.u) % p.p;
            BigInteger u_denominator = (1 + multiCoord)%p.p;
            BigInteger u = u_numerator * Operations.ExtendedEuclid(p.p, u_denominator) % p.p;

            BigInteger v_numerator = (p.v * q.v - p.u * q.u)%p.p;
            while (v_numerator < 0)
                v_numerator += p.p;
            BigInteger v_denominator = (1 - multiCoord);
            while (v_denominator < 0)
                v_denominator += p.p;
            BigInteger v = v_numerator * Operations.ExtendedEuclid(p.p, v_denominator) % p.p;

            return new EdwardsCurveDecartPoint(u, v, p.p, p.e, p.d);


        }

        public EdwardsCurveDecartPoint MultiplyPoint(EdwardsCurveDecartPoint point, BigInteger N)
        {
            EdwardsCurveDecartPoint result = new EdwardsCurveDecartPoint(0, 1, point.p, point.e, point.d);
            var tmp = point;
            while (N != 0)
            {
                if (N % 2 == 1)
                {
                    result = AdditionPoints(tmp, result);
                }
                tmp = AdditionPoints(tmp, tmp);
                N = N / 2;
            }
            return result;
        }
    }
}
