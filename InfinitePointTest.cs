using System.Linq;
using System.Numerics;

namespace GOST_34._10_12
{
    static public class InfinitePointTest
    {
        // для шага 6 и 7 используем проективные координаты из (x,y) переводим в (x,y,z)
        // В шаге 6 нам надо узнать будет ли enumN * (x,y) бексонечно удаленной точкой,
        // для этого мы будем умножать точку на число в наших проективных координатах (для этого использую дихотомический алгоритм),
        // естественно все с приведением по модулю  если в результате у нас получится коордианата Z = 0 (mod p),
        // значит точка является бесконечно удаленной, она нам подходит, мы радуемся!
        public static ProjectivePoint DichotomousAlgorithm(ProjectivePoint P, BigInteger N, BigInteger r, BigInteger mod, BigInteger koeffA)
        {
            //Int64 value = Convert.ToInt64(enumN / r);
            string binary = BinaryNumber(N / r);
            int[] mS = new int[binary.Length];
            ProjectivePoint[] mP = new ProjectivePoint[binary.Length];
            for (int i = 0; i < binary.Length; i++)
            {
                if (binary[i] == '1')
                    mS[i] = 1;
                else
                    mS[i] = 0;
            }
            ProjectivePoint current = P;
            ProjectivePoint Q = P;
            for (int i = 1; i < binary.Length; i++)
            {
                current = DoublePointInProjectiveCoordinate(koeffA, current, mod);
                if (mS[i] == 1)
                {
                    Q = AddPointInProjectiveCoordinate(Q, current, mod);
                }
            }
            if (Q.X < 0)
            {
                Q.X += mod;
            }
            if (Q.Y < 0)
            {
                Q.Y += mod;
            }
            if (Q.Z < 0)
            {
                Q.Z += mod;
            }
            return Q;
            // сделать перевод в афинные координаты

        }
        // Метод - удвоение точки в проективных координатах по модулю P
        public static ProjectivePoint DoublePointInProjectiveCoordinate(BigInteger a, ProjectivePoint P, BigInteger mod)
        {
            BigInteger U = (2 * P.Y * P.Z) % mod;
            BigInteger T = (3 * P.X * P.X + a * P.Z * P.Z) % mod;
            BigInteger V = (2 * U * P.X * P.Y) % mod;
            BigInteger W = (T * T - 2 * V) % mod;

            BigInteger newX = (W * U) % mod;
            BigInteger newY = (T * (V - W) - 2 * U * U * P.Y * P.Y) % mod;
            BigInteger newZ = (U * U * U) % mod;

            return new ProjectivePoint { X = newX, Y = newY, Z = newZ };
        }

        // Метод - сложение двух точек в проективных координатах по модулю P
        public static ProjectivePoint AddPointInProjectiveCoordinate(ProjectivePoint P1, ProjectivePoint P2, BigInteger mod)
        {
            BigInteger U1 = (P1.X * P2.Z) % mod;
            BigInteger U2 = (P2.X * P1.Z) % mod;

            BigInteger T1 = (P1.Y * P2.Z) % mod;
            BigInteger T2 = (P2.Y * P1.Z) % mod;

            BigInteger U = (U1 - U2) % mod;
            BigInteger T = (T1 - T2) % mod;
            BigInteger V = (P1.Z * P2.Z) % mod;
            BigInteger W = (T * T * V - (U1 + U2) * U * U) % mod;

            BigInteger newX = (W * U) % mod;
            BigInteger newY = (T * (U1 * U * U - W) - T1 * U * U * U) % mod;
            BigInteger newZ = (V * U * U * U) % mod;

            return new ProjectivePoint { X = newX, Y = newY, Z = newZ };
        }

        // Метод - перевод числа из десятичного в двоичное (нужно для дихотомического алгоритма) 
        private static string BinaryNumber(BigInteger x)
        {
            string s = "";
            while (x > 0)
            {
                s = ((x % 2 == 0) ? "0" : "1") + s;
                x /= 2;
            }
            return new string(s.Reverse().ToArray());
            // return s;
        }


        private static BigInteger InvertMod(BigInteger a, BigInteger p)
        {
            BigInteger ex = p - 2, result = 1;
            while (ex > 0)
            {
                if (ex % 2 == 1)
                {
                    result = (result * a) % p;
                }
                a = (a * a) % p;
                ex /= 2;
            }
            return result;
        }
        // Преобразовываем из проективных координат в декартовы: X = X/Z; Y=Y/Z;
        //Так как у нас есть модуль, то просто находим обратный элемент к Z и получаем: 
        // X= X * Z^-1; Y=Y* Z^-1 (mod p)
        public static SimplePoint TranslationToSimplePoint(ProjectivePoint P, BigInteger mod)
        {
            BigInteger RevertZ = InvertMod(P.Z, mod);

            return new SimplePoint { X = (P.X * RevertZ) % mod, Y = (P.Y * RevertZ) % mod };
        }





    }

    public class ProjectivePoint
    {
        public BigInteger X { get; set; }
        public BigInteger Y { get; set; }
        public BigInteger Z { get; set; }
    }

    public class SimplePoint
    {
        public BigInteger X { get; set; }
        public BigInteger Y { get; set; }
    }


}
