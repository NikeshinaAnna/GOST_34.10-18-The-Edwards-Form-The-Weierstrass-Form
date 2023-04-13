using System;
using System.Numerics;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.Diagnostics;

namespace GOST_34._10_12
{
    class Program
    {
        static void Main()
        {
            BigInteger p, a, b, d, m, x, y, u, v, q;
            //чтение данных из файла
            using (var sr= new StreamReader("data/parameters.txt"))
            {
                string[] parameters = new string[10];
                for (int i = 0; i < 10; i++)
                {
                    var str = sr.ReadLine().Substring(4);
                    parameters[i] = str;
                }
                p = new BigInteger(parameters[0], 16);
                a = new BigInteger(parameters[1], 16);
                b = new BigInteger(parameters[2], 16);
                m = new BigInteger(parameters[3], 16);
                q = new BigInteger(parameters[4], 16);
                u = new BigInteger(parameters[5], 16); 
                v = new BigInteger(parameters[6], 16);
                d = new BigInteger(parameters[7], 10);
                x = new BigInteger(parameters[8], 16);
                y = new BigInteger(parameters[9], 16);
                d = d % p;
            }
            var P_w = new EllipticCurvePoint(a, b, x, y, p);
            var Q_w = (EllipticCurvePoint)P_w.MultiplyPointByNumber(d);

            var P_ed = new EdwardsCurvePoint(p, u, v);
            var Q_ed = (EdwardsCurvePoint)P_ed.MultiplyPointByNumber(d);

            var configReader = new AppSettingsReader();
            string pathToFiles = configReader.GetValue("path", typeof(string)).ToString();

            var root_directory = new DirectoryInfo(pathToFiles);
            var directories = root_directory.GetDirectories();
            string filename;
            foreach(var dir in directories)
            {
                var files = dir.GetFiles();
                foreach(var file in files)
                {
                    filename = file.DirectoryName+"\\"+file.Name;
                    Console.WriteLine();
                    Console.WriteLine("---Файл: "+filename);
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();
                    var sign_onWeirshtras = GenerateSign(p, d, P_w, q, filename);
                    stopWatch.Stop();
                    using (var sw = new StreamWriter("sign.txt"))
                        sw.Write(sign_onWeirshtras);
                    Console.WriteLine("Подпись на кривых Вейерштраса записана в файл! Time: {0}", stopWatch.Elapsed);
                   
                    stopWatch.Restart();
                    var sign_onEdwards = GenerateSign(p, d, P_ed, q, filename);
                    stopWatch.Stop();
                    using (var sw = new StreamWriter("sign.txt"))
                        sw.Write(sign_onEdwards);
                    Console.WriteLine("Подпись на кривой Эдвардса записана в файл! Time: {0}", stopWatch.Elapsed);
                }    
            }

            //var str_sign_onWeirshtras = File.ReadAllText("sign.txt");
            //Console.WriteLine(VerifySign(str_sign_onWeirshtras, q, filename, P_w, Q_w));
            //Console.WriteLine();

            //var str_sign_onEdwards = File.ReadAllText("sign.txt");
            //Console.WriteLine(VerifySign(str_sign_onEdwards, q, filename, P_ed, Q_ed));
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
                var byteSize = rnd.Next(1,64);//здесь ограничение, потому что bytesize!=0
                byte[] bytes = new byte[byteSize];
                RNG.GetBytes(bytes);
                var bitString = string.Concat(bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
                k = new BigInteger(bitString, 2);
            } while (k < 0 || k > q);

            return k;
        }
        public static string GenerateSign(BigInteger p, BigInteger d, ICurve P, BigInteger q, string filename)
        {
            //byte[] message = Encoding.Default.GetBytes(File.ReadAllText("Book.pdf"));//пока не решила, как лучше читать биты из сообщения
            byte[] message = File.ReadAllBytes(filename);
            Streebog streebog = new Streebog(256);
            var h = streebog.GetHash(message);

            var result = string.Concat(h.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

            BigInteger binary_alpha = new BigInteger(result, 2);
            BigInteger e = binary_alpha % q;
            if (e == 0)
                e = 1;

            RandomNumberGenerator RNG = RandomNumberGenerator.Create();
            var rnd = new Random();
            BigInteger r = 0;
            BigInteger k;
            BigInteger s;
            do
            {
                k = GenerateK(q, rnd, RNG);
                if (P is EdwardsCurvePoint)
                {
                    var C = new EdwardsCurvePoint();
                    C = (EdwardsCurvePoint)P.MultiplyPointByNumber(k);
                    r = C.u % q;
                }
                else if (P is EllipticCurvePoint)
                {
                    var C = new EllipticCurvePoint();
                    C = (EllipticCurvePoint)P.MultiplyPointByNumber(k);
                    r = C.x % q;
                }
                s = (r * d + k * e) % q;
            } while (r == 0 || s == 0);

            string r_binary = r.ToString(2);
            string s_binary = s.ToString(2);

            var sign = ConcatTwoString(r_binary, s_binary);
            return sign;
        }

        public static bool VerifySign(string sign, BigInteger q, string path, ICurve P, ICurve Q)
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

            var v = Operations.ExtendedEuclid(q, e);
            var z1 = s * v % q;
            var z2 = q + ((-(r * v)) % q);
            BigInteger R = 0;

            if (P is EdwardsCurvePoint)
            {
                var A = (EdwardsCurvePoint)P.MultiplyPointByNumber(z1);
                var B = (EdwardsCurvePoint)Q.MultiplyPointByNumber(z2);
                var C = (EdwardsCurvePoint)A.AddPoints(B);
                R = C.u % q;
            }
            else if (P is EllipticCurvePoint)
            {
                var A = (EllipticCurvePoint)P.MultiplyPointByNumber(z1);
                var B = (EllipticCurvePoint)Q.MultiplyPointByNumber(z2);
                var C = (EllipticCurvePoint)A.AddPoints(B);
                R = C.x % q;
            }
            
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
            var C_edw = (EdwardsCurvePoint)A_edw.AddPoints(B_edw);
            var D_edw = (EdwardsCurvePoint)A_edw.MultiplyPointByNumber(4582748486);
            Console.WriteLine();
        }
    }
}
