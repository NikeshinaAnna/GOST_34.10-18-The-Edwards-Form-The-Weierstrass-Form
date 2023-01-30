using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Security;
using System.Security.Cryptography;

namespace GOST_34._10_12
{
    class Program
    {
        static void Main()
        {
            BigInteger p = new BigInteger("57896044618658097711785492504343953926634992332820282019728792003956564821041", 10);
            BigInteger a = new BigInteger("7", 10);
            BigInteger b = new BigInteger("43308876546767276905765904595650931995942111794451039583252968842033849580414", 10);
            BigInteger P_x = new BigInteger("2", 10);
            BigInteger P_y = new BigInteger("4018974056539037503335449422937059775635739389905545080690979365213431566280", 10);
            BigInteger Q_x = new BigInteger("57520216126176808443631405023338071176630104906313632182896741342206604859403", 10);
            BigInteger Q_y = new BigInteger("17614944419213781543809391949654080031942662045363639260709847859438286763994", 10);
            //BigInteger k = new BigInteger("53854137677348463731403841147996619241504003434302020712960838528893196233395", 10);
            BigInteger d = new BigInteger(" 55441196065363246126355624130324183196576709222340016572108097750006097525544", 10);

            var P = new EllipticCurvePoint(a, b, P_x, P_y, p);
            var Q = new EllipticCurvePoint(a, b, Q_x, Q_y, p);

            Console.WriteLine(VerifySign(GenerateSign(p, d, P), p, "Book.pdf", P, Q));
            Console.ReadKey();
        }
        public static string ConcatTwoString (string r, string s)
        {
            var r_length = r.Length;
            var s_length = s.Length;
            if (r.Length > s.Length)
                s = s.PadLeft(r_length, '0');
            else
                r = r.PadLeft(s_length, '0');
            return String.Concat(r, s);
        }

        public static BigInteger GenerateK(BigInteger p, Random rnd, RandomNumberGenerator RNG)
        {
            BigInteger k;
            do
            {
                var byteSize = rnd.Next(64);
                byte[] bytes = new byte[byteSize];
                RNG.GetBytes(bytes);
                var bitString = string.Concat(bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
                k = new BigInteger(bitString, 2);
            } while (k < 0 || k > p);

            return k;
        }

        public static string GenerateSign(BigInteger p, BigInteger d, EllipticCurvePoint P)
        {
            var C = new EllipticCurvePoint();
            // C = ProectiveECPoint.GetAffineECPoint(ProectiveECPoint.MultiplyPoint(new ProectiveECPoint(P), k));


            //byte[] message = Encoding.Default.GetBytes(File.ReadAllText("Book.pdf"));
            byte[] message = File.ReadAllBytes("Book.pdf");
            Streebog streebog = new Streebog(256);
            var h = streebog.GetHash(message);

            //var resSB = new StringBuilder();
            //for (int i = 0; i < h.Length; i++)
            //{
            //    var binaryView = Convert.ToString(h[i], 2).PadLeft(8, '0');
            //    resSB.Append(binaryView);
            //}
            //var resSTR = resSB.ToString();
            var result = string.Concat(h.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

            BigInteger binary_alpha = new BigInteger(result, 2);
            BigInteger e = binary_alpha % p;
            if (e == 0)
                e = 1;

            RandomNumberGenerator RNG = RandomNumberGenerator.Create();
            var rnd = new Random();
            BigInteger r;
            BigInteger k;
            BigInteger s;

            do
            {
                k = GenerateK(p, rnd, RNG);
                C = ProectiveECPoint.GetAffineECPoint(ProectiveECPoint.MultiplyPoint(new ProectiveECPoint(P), k));
                r = C.x % p;
                s = (r * d + k * e) % p;
            } while (r == 0 || s == 0);

            string r_binary = r.ToString(2);
            string s_binary = s.ToString(2);

            var sign = ConcatTwoString(r_binary, s_binary);
            return sign;
        }

        public static bool VerifySign(string sign, BigInteger p,string path, EllipticCurvePoint P, EllipticCurvePoint Q)
        {
            var r_binaryStr = sign.Substring(0, sign.Length / 2);
            var s_binaryStr = sign.Substring(sign.Length / 2);
            var r = new BigInteger(r_binaryStr, 2);
            var s = new BigInteger(s_binaryStr, 2);
            if (r > p || r < 0 || s < 0 || s > p)
                return false;
            byte[] message = File.ReadAllBytes(path);
            Streebog streebog = new Streebog(256);
            var h = streebog.GetHash(message);

            var result = string.Concat(h.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

            BigInteger binary_alpha = new BigInteger(result, 2);
            BigInteger e = binary_alpha % p;
            if (e == 0)
                e = 1;

            var v = ProectiveECPoint.ExtendedEuclid(p, e);
            v = v % p;

            var z1 = s * v % p;
            var z2 = p + ((-(r * v)) % p);

            var C= ProectiveECPoint.GetAffineECPoint(ProectiveECPoint.AdditionPoints
                                    (ProectiveECPoint.MultiplyPoint(new ProectiveECPoint(P), z1),ProectiveECPoint.MultiplyPoint(new ProectiveECPoint(Q), z2)));
            var R = C.x % p;
            if (R == r)
                return true;
            else return false;
        }
    }
}
