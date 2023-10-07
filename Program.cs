using System;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using GOST_34._10_12;

namespace GOST_34._10_18
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
                    var str = sr.ReadLine()[4..];
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
            var P = new WeierstrassCurveDecartPoint(P_w);
            var Q = P.MultiplyPoint(P, d);

            var P_ed = new EdwardsCurvePoint(p, u, v);
            var Q_ed = (EdwardsCurvePoint)P_ed.MultiplyPointByNumber(d);
            var P_e = new EdwardsCurveDecartPoint(P_ed);
            var Q_e = P_e.MultiplyPoint(P_e, d);

            var configReader = new AppSettingsReader();
            string pathToFiles = configReader.GetValue("path", typeof(string)).ToString();

            var root_directory = new DirectoryInfo(pathToFiles);
            var directories = root_directory.GetDirectories();
            using (var sw = new StreamWriter("results_sign_"+DateTime.Today.Day.ToString()+".txt"))
            {
                foreach (var dir in directories)
                {
                    var files = dir.GetFiles( "*", SearchOption.AllDirectories);
                    SignExperimentWithFiles(p, d, q, P_w, P_ed, files, sw);
                }
            }
            using (var sw = new StreamWriter("results_verify_" + DateTime.Today.Day.ToString() + ".txt"))
            {
                foreach (var dir in directories)
                {
                    var files = dir.GetFiles("*", SearchOption.AllDirectories);
                    VerifyExperimentWithFiles(q, P_w, P_ed, Q_w, Q_ed, files, sw);
                }
            }
        }

        private static void SignExperimentWithFiles(BigInteger p, BigInteger d, BigInteger q, EllipticCurvePoint P_w, EdwardsCurvePoint P_ed, FileInfo[] files, StreamWriter sw)
        {
            string filename;
            foreach (var file in files)
            {
                filename = file.DirectoryName + "\\" + file.Name;
                sw.WriteLine("----------------------------------------------");
                sw.WriteLine("---Файл: " + filename);
                sw.WriteLine("----------------------------------------------");
                var stopWatch = new Stopwatch();
                var fileNameOfWeirshtrassSign = "WSign_" + Path.GetFileNameWithoutExtension(filename)+".txt";
                stopWatch.Start();
                var sign_onWeirshtras = CreateAndVerifySign.GenerateSign(p, d, P_w, q, filename);
                stopWatch.Stop();
                using (var sw_sign = new StreamWriter(fileNameOfWeirshtrassSign))
                    sw_sign.Write(sign_onWeirshtras);
                sw.WriteLine("В форме Вейерштрасса: {0}", stopWatch.Elapsed);

                stopWatch.Reset();
                stopWatch.Start();
                var sign_onEdwards = CreateAndVerifySign.GenerateSign(p, d, P_ed, q, filename);
                stopWatch.Stop();
                var fileNameOfEdwardsSign = "EdSign_" + Path.GetFileNameWithoutExtension(filename) + ".txt";
                using (var sw_sign = new StreamWriter(fileNameOfEdwardsSign))
                    sw_sign.Write(sign_onEdwards);
                sw.WriteLine("В форме Эдвардса: {0}", stopWatch.Elapsed);
                sw.WriteLine();
            }
        }

        private static void VerifyExperimentWithFiles(BigInteger q, EllipticCurvePoint P_w, EdwardsCurvePoint P_ed, EllipticCurvePoint Q_w, EdwardsCurvePoint Q_ed, FileInfo[] files, StreamWriter sw)
        {
            string filename;
            foreach (var file in files)
            {
                filename = file.DirectoryName + "\\" + file.Name;
                sw.WriteLine("----------------------------------------------");
                sw.WriteLine("---Файл: " + filename);
                sw.WriteLine("----------------------------------------------");
                var stopWatch = new Stopwatch();
                var fileNameOfWeirshtrassSign = "WSign_" + Path.GetFileNameWithoutExtension(filename) + ".txt";
                var str_sign_onWeirshtras = File.ReadAllText(fileNameOfWeirshtrassSign);
                stopWatch.Start();
                CreateAndVerifySign.VerifySign(str_sign_onWeirshtras, q, filename, P_w, Q_w);
                stopWatch.Stop();
                sw.WriteLine("В форме Вейерштрасса: {0}", stopWatch.Elapsed);

                stopWatch.Reset();
                var fileNameOfEdwardsSign = "EdSign_" + Path.GetFileNameWithoutExtension(filename) + ".txt";
                var str_sign_onEdwards = File.ReadAllText(fileNameOfEdwardsSign);
                stopWatch.Start();
                CreateAndVerifySign.VerifySign(str_sign_onEdwards, q, filename, P_ed, Q_ed);
                stopWatch.Stop();
                sw.WriteLine("В форме Эдвардса: {0}", stopWatch.Elapsed);
                sw.WriteLine();
            }
        }
    }
}
