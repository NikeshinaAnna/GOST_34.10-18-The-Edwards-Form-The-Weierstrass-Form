using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GOST_34._10_12
{
    public class EllipticCurvePoint
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
    }
    class ProectiveECPoint
    {
        public BigInteger x { get; set; }
        public BigInteger y { get; set; }
        public BigInteger z { get; set; }
        public BigInteger p { get; set; }
        public BigInteger a { get; set; }
        public BigInteger b { get; set; }

        public ProectiveECPoint(EllipticCurvePoint ellipticPoint)
        {
            x = ellipticPoint.x;
            y = ellipticPoint.y;
            z = 1;
            p = ellipticPoint.fieldCharacteristic;
            a = ellipticPoint.a;
            b = ellipticPoint.b;
        }
        public ProectiveECPoint(BigInteger p_x, BigInteger p_y, BigInteger p_z, BigInteger p_p, BigInteger p_a, BigInteger p_b)
        {
            x = p_x;
            y = p_y;
            z = p_z;
            p = p_p;
            a = p_a;
            b = p_b;
        }

        public static ProectiveECPoint AdditionPoints(ProectiveECPoint _point1, ProectiveECPoint _point2)
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

            return new ProectiveECPoint(x_new, y_new, z_new,fc,_point1.a, _point1.b);
        }

        private static ProectiveECPoint DoublePoint(ProectiveECPoint _point)
        {
            var fc = _point.p;
            var U = 2 * _point.y * _point.z % fc;
            var T = (3 * _point.x * _point.x + _point.a * _point.z * _point.z) % fc;
            var V = 2 * U * _point.x * _point.y % fc;
            var W = (T * T - 2 * V) % fc;
            BigInteger x_new = (W * U );
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

            return new ProectiveECPoint(x_new, y_new, z_new,fc,_point.a, _point.b);
        }

        public static ProectiveECPoint MultiplyPoint(ProectiveECPoint _point, BigInteger N)
        {
            ProectiveECPoint result = new ProectiveECPoint(0, 1, 0,_point.p,_point.a, _point.b);
            var tmp = _point;
            //byte [] N_bits;
            //var str = Convert.ToString(N);
            //N_bits = N.getBytes();//представляем наш коэфициент в двоичном виде
            while(N!=0)
            {
                if(N%2==1)
                {
                    result = AdditionPoints(tmp, result);
                }
                tmp = DoublePoint(tmp);
                N = N / 2;
            }
            //for (int i = N_bits.Length - 1; i >= 0; i--)
            //{
            //    if (N_bits[i] == 1)
            //    {
            //        result = AdditionPoints(tmp, result);
            //    }
            //    tmp = DoublePoint(tmp);
            //}
            Console.WriteLine(result.x.ToString() + ' ' + result.y.ToString() + ' ' + result.z.ToString());
            return result;
        }


        public static EllipticCurvePoint GetAffineECPoint(ProectiveECPoint _point)
        {
            var coef = ExtendedEuclid(_point.p, _point.z);
            BigInteger _x = _point.x * coef % _point.p;
            BigInteger _y = _point.y * coef % _point.p;

            return new EllipticCurvePoint(_point.a, _point.b, _x, _y, _point.p);
        }

        //это для поиска обратного
        public static BigInteger ExtendedEuclid(BigInteger base_n, BigInteger number)
        {
            var a = base_n;
            BigInteger res = 0;
            BigInteger w = 1;
            BigInteger temp;
            BigInteger temp1;
            BigInteger x;
            number = number + base_n;
            number = number % base_n;

            while (number > 0) // 
            {
                temp1 = a / number;
                x = number;
                number = a % x;
                a = x;
                x = w;
                temp = temp1 * x;
                w = res - temp;
                res = x;
            }
            res %= base_n;
            x = res + base_n;
            res = x % base_n;
            return res;
        }
    }
}
