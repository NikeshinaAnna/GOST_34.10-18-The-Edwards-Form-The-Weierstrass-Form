namespace GOST_34._10_18
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

        public ICurve MultiplyPointByNumber(BigInteger number)
        {
            var proectivePoint = new EdwardsCurveProjectivePoint(this);
            return EdwardsCurveProjectivePoint.GetAffineEdwardsPoint(EdwardsCurveProjectivePoint.MultiplyEdwardsProectivePoint(proectivePoint, number));
        }

        public ICurve AddPoints(ICurve point2)
        {
            var proectivePointA = new EdwardsCurveProjectivePoint(this);
            var proectivePointB = new EdwardsCurveProjectivePoint(point2 as EdwardsCurvePoint);
            return EdwardsCurveProjectivePoint.GetAffineEdwardsPoint(EdwardsCurveProjectivePoint.AdditionPoints(proectivePointA, proectivePointB));
        }
    }
}
