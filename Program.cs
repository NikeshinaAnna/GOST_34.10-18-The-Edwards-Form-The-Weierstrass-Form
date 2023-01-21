using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace GOST_34._10_12
{
    class Program
    {
        static void Main()
        {
            BigInteger p= new BigInteger("57896044618658097711785492504343953926634992332820282019728792003956564821041", 10);
            BigInteger a = new BigInteger("7", 10);
            BigInteger b = new BigInteger("43308876546767276905765904595650931995942111794451039583252968842033849580414", 10);
            BigInteger P_x = new BigInteger("2", 10);
            BigInteger P_y = new BigInteger("4018974056539037503335449422937059775635739389905545080690979365213431566280", 10);
            BigInteger Q_x = new BigInteger("57520216126176808443631405023338071176630104906313632182896741342206604859403", 10);
            BigInteger Q_y = new BigInteger("17614944419213781543809391949654080031942662045363639260709847859438286763994", 10);
            BigInteger k = new BigInteger("53854137677348463731403841147996619241504003434302020712960838528893196233395", 10);

            var P = new EllipticCurvePoint(a, b, P_x, P_y, p);
            var C = new EllipticCurvePoint();
            C = ProectiveECPoint.GetAffineECPoint(ProectiveECPoint.MultiplyPoint(new ProectiveECPoint(P), k));
     

            byte[] message = File.ReadAllBytes("Book.pdf");
            Streebog streebog = new Streebog(512);
            var h = streebog.GetHash(message);
            Console.ReadKey();
        }
    }
}
