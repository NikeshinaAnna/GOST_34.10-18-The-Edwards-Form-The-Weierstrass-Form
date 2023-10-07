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
            var proectivePointA = new WeierstrassCurveProjectivePoint(this);
            var proectivePointB = new WeierstrassCurveProjectivePoint(point2 as EllipticCurvePoint);
            return WeierstrassCurveProjectivePoint.GetAffineECPoint(WeierstrassCurveProjectivePoint.AdditionPoints(proectivePointA, proectivePointB));
        }

        public ICurve MultiplyPointByNumber(BigInteger number)
        {
            var proectivePoint = new WeierstrassCurveProjectivePoint(this);
            return WeierstrassCurveProjectivePoint.GetAffineECPoint(WeierstrassCurveProjectivePoint.MultiplyPoint(proectivePoint, number));
        }
    }
}
