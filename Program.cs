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
            //BigInteger p = new BigInteger("57896044618658097711785492504343953926634992332820282019728792003956564821041", 10);
            //BigInteger a = new BigInteger("7", 10);
            //BigInteger b = new BigInteger("43308876546767276905765904595650931995942111794451039583252968842033849580414", 10);
            //BigInteger P_x = new BigInteger("2", 10);
            //BigInteger P_y = new BigInteger("4018974056539037503335449422937059775635739389905545080690979365213431566280", 10);
            //BigInteger Q_x = new BigInteger("57520216126176808443631405023338071176630104906313632182896741342206604859403", 10);
            //BigInteger Q_y = new BigInteger("17614944419213781543809391949654080031942662045363639260709847859438286763994", 10);
            //BigInteger d = new BigInteger(" 55441196065363246126355624130324183196576709222340016572108097750006097525544", 10);
            //var q = new BigInteger("57896044618658097711785492504343953927082934583725450622380973592137631069619", 10);

            BigInteger p = new BigInteger("3623986102229003635907788753683874306021320925534678605086546150450856166624002482588482022271496854025090823603058735163734263822371964987228582907372403", 10);
            BigInteger a = new BigInteger("7", 10);
            BigInteger b = new BigInteger("1518655069210828534508950034714043154928747527740206436194018823352809982443793732829756914785974674866041605397883677596626326413990136959047435811826396", 10);
            BigInteger P_x = new BigInteger("1928356944067022849399309401243137598997786635459507974357075491307766592685835441065557681003184874819658004903212332884252335830250729527632383493573274", 10);
            BigInteger P_y = new BigInteger("2288728693371972859970012155529478416353562327329506180314497425931102860301572814141997072271708807066593850650334152381857347798885864807605098724013854", 10);
            BigInteger Q_x = new BigInteger("909546853002536596556690768669830310006929272546556281596372965370312498563182320436892870052842808608262832456858223580713780290717986855863433431150561", 10);
            BigInteger Q_y = new BigInteger("2921457203374425620632449734248415455640700823559488705164895837509539134297327397380287741428246088626609329139441895016863758984106326600572476822372076", 10);
            BigInteger d = new BigInteger("610081804136373098219538153239847583006845519069531562982388135354890606301782255383608393423372379057665527595116827307025046458837440766121180466875860", 10);
            var q = new BigInteger("3623986102229003635907788753683874306021320925534678605086546150450856166623969164898305032863068499961404079437936585455865192212970734808812618120619743", 10);

            var P = new EllipticCurvePoint(a, b, P_x, P_y, p);
            var Q = new EllipticCurvePoint(a, b, Q_x, Q_y, p);

            var sign = GenerateSign(p, d, P, q);
            using (var sw = new StreamWriter("sign.txt"))
            {
                sw.Write(sign);
            }
            Console.WriteLine("Подпись записана в файл!");
            var str = File.ReadAllText("sign.txt");
            Console.WriteLine(VerifySign(str, q, "Book.pdf", P, Q));
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

        public static string GenerateSign(BigInteger p, BigInteger d, EllipticCurvePoint P,BigInteger q)
        {
            var C = new EllipticCurvePoint();
            //проверяю правильность умножения
            //BigInteger k = new BigInteger("53854137677348463731403841147996619241504003434302020712960838528893196233395", 10);
            //BigInteger k = new BigInteger("175516356025850499540628279921125280333451031747737791650208144243182057075034446102986750962508909227235866126872473516807810541747529710309879958632945", 10);
            //C = ProectiveECPoint.GetAffineECPoint(ProectiveECPoint.MultiplyPoint(new ProectiveECPoint(P), k));


            //byte[] message = Encoding.Default.GetBytes(File.ReadAllText("Book.pdf"));//пока не решила, как лучше читать биты из сообщения
            byte[] message = File.ReadAllBytes("Book.pdf");
            Streebog streebog = new Streebog(256);
            var h = streebog.GetHash(message);

            //сверху аналогичный участок кода перевода byte в битовую строку, просто без использования linq
            
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

        public static bool VerifySign(string sign, BigInteger q,string path, EllipticCurvePoint P, EllipticCurvePoint Q)
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
            //e = new BigInteger("20798893674476452017134061561508270130637142515379653289952617252661468872421", 10);
            
            var v = ProectiveECPoint.ExtendedEuclid(q, e);
           // var v2 = e.modInverse(p);
            //v = v % q;

            var z1 = s * v % q;
            var z2 = q + ((-(r * v)) % q);

            var A = ProectiveECPoint.MultiplyPoint(new ProectiveECPoint(P), z1);
            var B = ProectiveECPoint.MultiplyPoint(new ProectiveECPoint(Q), z2);

            var C= ProectiveECPoint.GetAffineECPoint(ProectiveECPoint.AdditionPoints(A,B));


            //var A_2 = InfinitePointTest.DichotomousAlgorithm(new ProjectivePoint() { X= P.x,Y= P.y,Z= 1 }, z1, 1, p, P.a);
            //var B_2 = InfinitePointTest.DichotomousAlgorithm(new ProjectivePoint() { X = Q.x, Y = Q.y, Z = 1 }, z2, 1, p, Q.a);
            //var C2 = InfinitePointTest.AddPointInProjectiveCoordinate(A_2, B_2, p);
            //var pOINT = new SimplePoint();
            //pOINT = InfinitePointTest.TranslationToSimplePoint(C2, p);
            var R = C.x % q;

            if (R == r)
                return true;
            else return false;
        }
    }
}
