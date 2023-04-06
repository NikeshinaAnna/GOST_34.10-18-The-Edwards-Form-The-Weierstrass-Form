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
            /*Первый набор данных из самого госта
             * BigInteger p = new BigInteger("57896044618658097711785492504343953926634992332820282019728792003956564821041", 10);
            BigInteger a = new BigInteger("7", 10);
            BigInteger b = new BigInteger("43308876546767276905765904595650931995942111794451039583252968842033849580414", 10);
            BigInteger P_x = new BigInteger("2", 10);
            BigInteger P_y = new BigInteger("4018974056539037503335449422937059775635739389905545080690979365213431566280", 10);
            BigInteger Q_x = new BigInteger("57520216126176808443631405023338071176630104906313632182896741342206604859403", 10);
            BigInteger Q_y = new BigInteger("17614944419213781543809391949654080031942662045363639260709847859438286763994", 10);
            BigInteger d = new BigInteger("55441196065363246126355624130324183196576709222340016572108097750006097525544", 10);
            var q = new BigInteger("57896044618658097711785492504343953927082934583725450622380973592137631069619", 10);*/

            //проверяю гипотезу, что Q=dP, выполняется
            /*var P = new EllipticCurvePoint(a, b, P_x, P_y, p);
            var C = new EllipticCurvePoint();
            C = ProectiveECPoint.GetAffineECPoint(ProectiveECPoint.MultiplyPoint(new ProectiveECPoint(P), d));*/
            //-------------

            /*var sign = GenerateSignOnEllipticCurves(p, d, P, q);
            using (var sw = new StreamWriter("sign.txt"))
            {
                sw.Write(sign);
            }
            Console.WriteLine("Подпись записана в файл!");
            var str = File.ReadAllText("sign.txt");
            Console.WriteLine(VerifySignOnEllipticCurves(str, q, "Book.pdf", P, Q));
            Console.WriteLine();*/

            /*Второй набор данных из госта
             * BigInteger p = new BigInteger("3623986102229003635907788753683874306021320925534678605086546150450856166624002482588482022271496854025090823603058735163734263822371964987228582907372403", 10);
            BigInteger a = new BigInteger("7", 10);
            BigInteger b = new BigInteger("1518655069210828534508950034714043154928747527740206436194018823352809982443793732829756914785974674866041605397883677596626326413990136959047435811826396", 10);
            BigInteger P_x = new BigInteger("1928356944067022849399309401243137598997786635459507974357075491307766592685835441065557681003184874819658004903212332884252335830250729527632383493573274", 10);
            BigInteger P_y = new BigInteger("2288728693371972859970012155529478416353562327329506180314497425931102860301572814141997072271708807066593850650334152381857347798885864807605098724013854", 10);
            BigInteger Q_x = new BigInteger("909546853002536596556690768669830310006929272546556281596372965370312498563182320436892870052842808608262832456858223580713780290717986855863433431150561", 10);
            BigInteger Q_y = new BigInteger("2921457203374425620632449734248415455640700823559488705164895837509539134297327397380287741428246088626609329139441895016863758984106326600572476822372076", 10);
            BigInteger d = new BigInteger("610081804136373098219538153239847583006845519069531562982388135354890606301782255383608393423372379057665527595116827307025046458837440766121180466875860", 10);
            var q = new BigInteger("3623986102229003635907788753683874306021320925534678605086546150450856166623969164898305032863068499961404079437936585455865192212970734808812618120619743", 10);
            var P = new EllipticCurvePoint(a, b, P_x, P_y, p);
            var Q = new EllipticCurvePoint(a, b, Q_x, Q_y, p);*/

            //Данные из спецификации
            var p = new BigInteger("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFD97", 16);
            var a = new BigInteger("C2173F1513981673AF4892C23035A27CE25E2013BF95AA33B22C656F277E7335", 16);
            var b = new BigInteger("295F9BAE7428ED9CCC20E7C359A9D41A22FCCD9108E17BF7BA9337A6F8AE9513", 16);
            var e = new BigInteger("01", 16);
            var d = new BigInteger("0605F6B7C183FA81578BC39CFAD518132B9DF62897009AF7E522C32D6DC7BFFB", 16);
            var m = new BigInteger("1000000000000000000000000000000003F63377F21ED98D70456BD55B0D8319C", 16);
            var q = new BigInteger("400000000000000000000000000000000FD8CDDFC87B6635C115AF556C360C67", 16);
            var u = new BigInteger("0D", 16);
            var v = new BigInteger("60CA1E32AA475B348488C38FAB07649CE7EF8DBE87F22E81F92B2592DBA300E7", 16);
            var k = new BigInteger("55441196065363246126355624130324183196576709222340016572108097750006097525544", 10);
            k = k % p;

            var P = new EdwardsCurvePoint(p, u, v);
            var Q = new EdwardsCurvePoint();
            Q=EdwardsCurvePoint.MultipluEdwardsPoint(k, P);

            var sign = GenerateSignOnEdwardsCurves(p, k, P, q);
            using (var sw = new StreamWriter("sign.txt"))
                sw.Write(sign);

            Console.WriteLine("Подпись записана в файл!");
            var str = File.ReadAllText("sign.txt");
            Console.WriteLine(VerifySignOnEdwardsCurves(str, q, "photo.jpg", P, Q));
            Console.WriteLine();
            
            //checkEllipticCurves();
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

        public static BigInteger GenerateK(BigInteger q, Random rnd, RandomNumberGenerator RNG)
        {
            BigInteger k;
            do
            {
                var byteSize = rnd.Next(64);
                byte[] bytes = new byte[byteSize];
                RNG.GetBytes(bytes);
                var bitString = string.Concat(bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
                k = new BigInteger(bitString, 2);
            } while (k < 0 || k > q);

            return k;
        }

        public static string GenerateSignOnEdwardsCurves(BigInteger p, BigInteger d, EdwardsCurvePoint P_edw, BigInteger q)
        {
            var C = new EdwardsCurvePoint();

            //byte[] message = Encoding.Default.GetBytes(File.ReadAllText("Book.pdf"));//пока не решила, как лучше читать биты из сообщения
            byte[] message = File.ReadAllBytes("photo.jpg");
            Streebog streebog = new Streebog(256);
            var h = streebog.GetHash(message);

            var result = string.Concat(h.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

            BigInteger binary_alpha = new BigInteger(result, 2);
            BigInteger e = binary_alpha % q;
            if (e == 0)
                e = 1;

            RandomNumberGenerator RNG = RandomNumberGenerator.Create();
            var rnd = new Random();
            BigInteger r;
            BigInteger k;
            BigInteger s;
            do
            {
                k = GenerateK(q, rnd, RNG);
                C = EdwardsCurvePoint.MultipluEdwardsPoint(k, P_edw);
                r = C.u % q;
                s = (r * d + k * e) % q;
            } while (r == 0 || s == 0);

            string r_binary = r.ToString(2);
            string s_binary = s.ToString(2);

            var sign = ConcatTwoString(r_binary, s_binary);
            return sign;
        }

        public static bool VerifySignOnEdwardsCurves(string sign, BigInteger q, string path, EdwardsCurvePoint P_edw, EdwardsCurvePoint Q_edw)
        {
            var r_binaryStr = sign.Substring(0, sign.Length / 2);
            var s_binaryStr = sign.Substring(sign.Length / 2);
            var r = new BigInteger(r_binaryStr, 2);
            var s = new BigInteger(s_binaryStr, 2);
            if (r > q || r < 0 || s < 0 || s > q)
                return false;
            byte[] message = File.ReadAllBytes(path);
            Streebog streebog = new Streebog(256);
            var h = streebog.GetHash(message);

            var result = string.Concat(h.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

            BigInteger binary_alpha = new BigInteger(result, 2);
            BigInteger e = binary_alpha % q;
            if (e == 0)
                e = 1;

            var v = ProectiveECPoint.ExtendedEuclid(q, e);
            var z1 = s * v % q;
            var z2 = q + ((-(r * v)) % q);

            var A = EdwardsCurvePoint.MultipluEdwardsPoint(z1, P_edw);
            var B = EdwardsCurvePoint.MultipluEdwardsPoint(z2, Q_edw);

            var C = EdwardsCurvePoint.SumEdwardsPoint(A, B);
            var R = C.u % q;

            if (R == r)
                return true;
            else return false;
        }

        public static string GenerateSignOnEllipticCurves(BigInteger p, BigInteger d, EllipticCurvePoint P,BigInteger q)
        {
            var C = new EllipticCurvePoint();

            //byte[] message = Encoding.Default.GetBytes(File.ReadAllText("Book.pdf"));//пока не решила, как лучше читать биты из сообщения
            byte[] message = File.ReadAllBytes("Book.pdf");
            Streebog streebog = new Streebog(256);
            var h = streebog.GetHash(message);
            
            var result = string.Concat(h.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

            BigInteger binary_alpha = new BigInteger(result, 2);
            BigInteger e = binary_alpha % q;
            if (e == 0)
                e = 1;

            RandomNumberGenerator RNG = RandomNumberGenerator.Create();
            var rnd = new Random();
            BigInteger r;
            BigInteger k;
            BigInteger s;
            do
            {
                k = GenerateK(q, rnd, RNG);
                C = ProectiveECPoint.GetAffineECPoint(ProectiveECPoint.MultiplyPoint(new ProectiveECPoint(P), k));
                r = C.x % q;
                s = (r * d + k * e) % q;
            } while (r == 0 || s == 0);

            string r_binary = r.ToString(2);
            string s_binary = s.ToString(2);

            var sign = ConcatTwoString(r_binary, s_binary);
            return sign;
        }

        public static bool VerifySignOnEllipticCurves(string sign, BigInteger q,string path, EllipticCurvePoint P, EllipticCurvePoint Q)
        {
            var r_binaryStr = sign.Substring(0, sign.Length / 2);
            var s_binaryStr = sign.Substring(sign.Length / 2);
            var r = new BigInteger(r_binaryStr, 2);
            var s = new BigInteger(s_binaryStr, 2);
            if (r > q || r < 0 || s < 0 || s > q)
                return false;
            byte[] message = File.ReadAllBytes(path);
            Streebog streebog = new Streebog(256);
            var h = streebog.GetHash(message);

            var result = string.Concat(h.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

            BigInteger binary_alpha = new BigInteger(result, 2);
            BigInteger e = binary_alpha % q;
            if (e == 0)
                e = 1;

            var v = ProectiveECPoint.ExtendedEuclid(q, e);
            var z1 = s * v % q;
            var z2 = q + ((-(r * v)) % q);

            var A = ProectiveECPoint.MultiplyPoint(new ProectiveECPoint(P), z1);
            var B = ProectiveECPoint.MultiplyPoint(new ProectiveECPoint(Q), z2);

            var C= ProectiveECPoint.GetAffineECPoint(ProectiveECPoint.AdditionPoints(A,B));
            var R = C.x % q;

            if (R == r)
                return true;
            else return false;
        }

        public static void checkEllipticCurves()
        {
            var p = new BigInteger("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFD97", 16);
            var q = new BigInteger("400000000000000000000000000000000FD8CDDFC87B6635C115AF556C360C67", 16);
            var a = new BigInteger("C2173F1513981673AF4892C23035A27CE25E2013BF95AA33B22C656F277E7335", 16);
            var b = new BigInteger("295F9BAE7428ED9CCC20E7C359A9D41A22FCCD9108E17BF7BA9337A6F8AE9513", 16);
            var e = new BigInteger("0000000000000000000000000000000000000000000000000000000000000001", 16);
            var d = new BigInteger("0605F6B7C183FA81578BC39CFAD518132B9DF62897009AF7E522C32D6DC7BFFB", 16);

            ////контрольный пример 1
            //var Ax = new BigInteger("91e38443a5e82c0d880923425712b2bb658b9196932e02c78b2582fe742daa28", 16);
            //var Ay = new BigInteger("32879423ab1a0375895786c4bb46e9565fde0b5344766740af268adb32322e5c", 16);

            //var Bx = new BigInteger("e8c6740e58d616ca220db7da0d9c3e19b53e86e38bf3e8747774631452ec174c", 16);
            //var By = new BigInteger("0b837a5e560a29a2327b575f29b4be8baef4bc947fcc2ed4f3264bc434309381", 16);

            //var A_weir = new EllipticCurvePoint(a, b, Ax, Ay, p);
            //var B_weir = new EllipticCurvePoint(a, b, Bx, By, p);
            //var C_weir = ProectiveECPoint.GetAffineECPoint(ProectiveECPoint.AdditionPoints(new ProectiveECPoint(A_weir), new ProectiveECPoint(B_weir)));
            //var D_weir= ProectiveECPoint.GetAffineECPoint(ProectiveECPoint.MultiplyPoint(new ProectiveECPoint(A_weir), 2));

            //контрольный пример 2
            var Ax = new BigInteger("1d40c1676805f9518be1fb4c7ae460d3608581e477b07c2d0e7e1e265a6b3347", 16);
            var Ay = new BigInteger("8291ace380fd8832baca29613ab5626c302d13348f204d727d30897a8e1f8934", 16);

            var Bx = new BigInteger("c9cbaeefaabc51147130fc6fa1adbe72140e35c5911b7d54b12beecdf5848943", 16);
            var By = new BigInteger("3c2e34cdfa1e6e76d6cc57ec871a26fb5b23d01c540e6f0d8f77f4d81fe3f613", 16);

            var A_weir = new EllipticCurvePoint(a, b, Ax, Ay, p);
            var B_weir = new EllipticCurvePoint(a, b, Bx, By, p);
            var C_weir = ProectiveECPoint.GetAffineECPoint(ProectiveECPoint.AdditionPoints(new ProectiveECPoint(A_weir), new ProectiveECPoint(B_weir)));
            var D_weir = ProectiveECPoint.GetAffineECPoint(ProectiveECPoint.MultiplyPoint(new ProectiveECPoint(A_weir), 4582748486));


            Console.WriteLine("Сложение: {0}; {1}", C_weir.x.ToHexString(), C_weir.y.ToHexString());
            Console.WriteLine("Удвоение: {0}; {1}", D_weir.x.ToHexString(), D_weir.y.ToHexString());

            var C_weir_edw = new EdwardsCurvePoint(C_weir);
            var D_weir_edw = new EdwardsCurvePoint(D_weir);

            var A_edw = new EdwardsCurvePoint(A_weir);
            var B_edw = new EdwardsCurvePoint(B_weir);
            var C_edw = EdwardsCurvePoint.SumEdwardsPoint(A_edw, B_edw);
            var D_edw = EdwardsCurvePoint.MultipluEdwardsPoint(4582748486, A_edw);
            Console.WriteLine();
        }
    }
}
