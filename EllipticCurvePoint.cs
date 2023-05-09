namespace GOST_34._10_18
{
    public class EllipticCurvePoint:ICurve
    {
        public BigInteger a;
        public BigInteger b;
        public BigInteger x;
        public BigInteger y;
        public BigInteger fieldCharacteristic;
        public EllipticCurvePoint() { }
        public EllipticCurvePoint(BigInteger _a, BigInteger _b, BigInteger _x, BigInteger _y, BigInteger _fieldCharacteristic)
        {
            a = _a;
            b = _b;
            x = _x;
            y = _y;
            fieldCharacteristic = _fieldCharacteristic;
        }

        public ICurve AddPoints(ICurve point2)
        {
            var proectivePointA = new EllipticCurveProjectivePoint(this);
            var proectivePointB = new EllipticCurveProjectivePoint(point2 as EllipticCurvePoint);
            return EllipticCurveProjectivePoint.GetAffineECPoint(EllipticCurveProjectivePoint.AdditionPoints(proectivePointA, proectivePointB));
        }

        public ICurve MultiplyPointByNumber(BigInteger number)
        {
            var proectivePoint = new EllipticCurveProjectivePoint(this);
            return EllipticCurveProjectivePoint.GetAffineECPoint(EllipticCurveProjectivePoint.MultiplyPoint(proectivePoint, number));
        }
    }
}
