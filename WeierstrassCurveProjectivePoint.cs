namespace GOST_34._10_18
{
    class WeierstrassCurveProjectivePoint
    {
        public BigInteger x { get; set; }
        public BigInteger y { get; set; }
        public BigInteger z { get; set; }
        public BigInteger p { get; set; }
        public BigInteger a { get; set; }
        public BigInteger b { get; set; }

        public WeierstrassCurveProjectivePoint(EllipticCurvePoint ellipticPoint)
        {
            x = ellipticPoint.x;
            y = ellipticPoint.y;
            z = 1;
            p = ellipticPoint.fieldCharacteristic;
            a = ellipticPoint.a;
            b = ellipticPoint.b;
        }
        public WeierstrassCurveProjectivePoint(BigInteger p_x, BigInteger p_y, BigInteger p_z, BigInteger p_p, BigInteger p_a, BigInteger p_b)
        {
            x = p_x;
            y = p_y;
            z = p_z;
            p = p_p;
            a = p_a;
            b = p_b;
        }

        public static WeierstrassCurveProjectivePoint AdditionPoints(WeierstrassCurveProjectivePoint _point1, WeierstrassCurveProjectivePoint _point2)
        {
            var fc = _point1.p;
            //var result = new ProectiveECPoint(0, 1, 0);//результат
            if (_point1.z == 0)
                return _point2;
            if (_point2.z == 0)
                return _point1;

            var T1 = _point1.y * _point2.z % fc;
            var T2 = _point1.z * _point2.y % fc;
            var U1 = _point1.x * _point2.z % fc;
            var U2 = _point1.z * _point2.x % fc;
            var U = (U1 - U2) % fc;
            var T = (T1 - T2) % fc;
            var V = _point2.z * _point1.z % fc;
            var W = (T * T * V - (U1 + U2) * U * U) % fc;

            BigInteger x_new = (W * U);
            x_new = x_new % fc;
            while (x_new < 0)
                x_new += fc;

            BigInteger y_new = (T * (U1 * U * U - W) - T1 * U * U * U);
            y_new = y_new % fc;
            while (y_new < 0)
                y_new += fc;

            BigInteger z_new = (V * U * U * U);
            z_new = z_new % fc;
            while (z_new < 0)
                z_new += fc;

            return new WeierstrassCurveProjectivePoint(x_new, y_new, z_new, fc, _point1.a, _point1.b);
        }

        private static WeierstrassCurveProjectivePoint DoublePoint(WeierstrassCurveProjectivePoint _point)
        {
            var fc = _point.p;
            var U = 2 * _point.y * _point.z % fc;
            var T = (3 * _point.x * _point.x + _point.a * _point.z * _point.z) % fc;
            var V = 2 * U * _point.x * _point.y % fc;
            var W = (T * T - 2 * V) % fc;
            BigInteger x_new = (W * U);
            x_new = x_new % fc;
            while (x_new < 0)
                x_new += fc;

            BigInteger y_new = (T * (V - W) - 2 * U * U * _point.y * _point.y);
            y_new = y_new % fc;
            while (y_new < 0)
                y_new += fc;

            BigInteger z_new = (U * U * U);
            z_new = z_new % fc;
            while (z_new < 0)
                z_new += fc;

            return new WeierstrassCurveProjectivePoint(x_new, y_new, z_new, fc, _point.a, _point.b);
        }

        public static WeierstrassCurveProjectivePoint MultiplyPoint(WeierstrassCurveProjectivePoint _point, BigInteger N)
        {
            WeierstrassCurveProjectivePoint result = new WeierstrassCurveProjectivePoint(0, 1, 0, _point.p, _point.a, _point.b);
            var tmp = _point;
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


        public static EllipticCurvePoint GetAffineECPoint(WeierstrassCurveProjectivePoint _point)
        {
            var coef = Operations.ExtendedEuclid(_point.p, _point.z);
            BigInteger _x = _point.x * coef % _point.p;
            BigInteger _y = _point.y * coef % _point.p;

            return new EllipticCurvePoint(_point.a, _point.b, _x, _y, _point.p);
        }
    }
}
