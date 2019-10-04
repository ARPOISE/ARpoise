using System;

namespace printPois
{
    public class Program
    {
        public static Random Random = new Random((int)DateTime.Now.Ticks);
        public static void PrintNaiGai(int id, double angle, double x, double y, double z, double size, double length)
        {
            var random = new Random(Random.Next() ^ (int)DateTime.Now.Ticks);
            angle = angle - 3 + random.Next(7);
            x = ((int)((x - 1.5 + random.NextDouble() * 3) * 1000)) / 1000.0;
            y = ((int)((y - 4 + random.NextDouble() * 12) * 1000)) / 1000.0;
            z = ((int)((z - 1.5 + random.NextDouble() * 3) * 1000)) / 1000.0;
            size = ((int)((size - .5 + random.NextDouble()) * 1000)) / 1000.0;
            length = ((int)((length - .5 + random.NextDouble()) * 1000)) / 1000.0;

            Console.WriteLine($"<poi><dimension>3</dimension><alt>0</alt><transform><rel/>");
            Console.WriteLine($"<angle>{angle}</angle><scale>1</scale></transform><object><baseURL>www.arpoise.com/AB/brushthesky.ace</baseURL>");
            Console.WriteLine($"<full>BtS_nai</full><poiLayerName/>");
            Console.WriteLine($"<relativeLocation>{x},{y},{z}</relativeLocation><icon/><size/><triggerImageURL/><triggerImageWidth>0</triggerImageWidth></object><relativeAlt>0</relativeAlt><animation events=\"onCreate\">");
            Console.WriteLine($"<name>naistart{id}</name><type>scale</type>");
            Console.WriteLine($"<length>0.25</length><delay>0</delay><interpolation>linear</interpolation><interpolationParam>0</interpolationParam><persist/><repeat/>");
            Console.WriteLine($"<from>0</from><to>{size}</to>");
            Console.WriteLine($"<followedBy>naion{id}</followedBy><axis>1,1,1</axis></animation><animation events=\"onFollow\">");
            Console.WriteLine($"<name>naion{id}</name><type>scale</type>");
            Console.WriteLine($"<length>{length}</length><delay>0</delay><interpolation>linear</interpolation><interpolationParam>0</interpolationParam><persist/><repeat/>");
            Console.WriteLine($"<from>{size}</from><to>{size}</to>");
            Console.WriteLine($"<followedBy>naihide{id}</followedBy><axis>1,1,1</axis></animation><animation events=\"onFollow\">");
            Console.WriteLine($"<name>naihide{id}</name><type>scale</type>");
            Console.WriteLine($"<length>0.25</length><delay>0</delay><interpolation>linear</interpolation><interpolationParam>0</interpolationParam><persist/><repeat/>");
            Console.WriteLine($"<from>{size}</from><to>0</to>");
            Console.WriteLine($"<followedBy>naioff{id}</followedBy><axis>1,1,1</axis></animation><animation events=\"onFollow\">");
            Console.WriteLine($"<name>naioff{id}</name><type>scale</type>");
            Console.WriteLine($"<length>{length}</length><delay>0</delay><interpolation>linear</interpolation><interpolationParam>0</interpolationParam><persist/><repeat/>");
            Console.WriteLine($"<from>0</from><to>0</to>");
            Console.WriteLine($"<followedBy>naishow{id}</followedBy><axis>1,1,1</axis></animation><animation events=\"onFollow\">");
            Console.WriteLine($"<name>naishow{id}</name><type>scale</type>");
            Console.WriteLine($"<length>0.25</length><delay>0</delay><interpolation>linear</interpolation><interpolationParam>0</interpolationParam><persist/><repeat/>");
            Console.WriteLine($"<from>0</from><to>{size}</to>");
            Console.WriteLine($"<followedBy>naion{id}</followedBy><axis>1,1,1</axis></animation><attribution/><distance/><visibilityRange>250</visibilityRange>");
            Console.WriteLine($"<id>{id}</id><imageURL/>");
            Console.WriteLine($"<lat>48.158487</lat><lon>11.57869</lon><line1/><line2/><line3/><line4/>");
            Console.WriteLine($"<title>nai {id}</title><type/><doNotIndex/><showSmallBiw>1</showSmallBiw><showBiwOnClick>1</showBiwOnClick><isVisible>1</isVisible><page>poi</page></poi>");
            Console.WriteLine("");

            Console.WriteLine($"<poi><dimension>3</dimension><alt>0</alt><transform><rel/>");
            Console.WriteLine($"<angle>{angle}</angle><scale>1</scale></transform><object><baseURL>www.arpoise.com/AB/brushthesky.ace</baseURL>");
            Console.WriteLine($"<full>BtS_gai</full><poiLayerName/>");
            Console.WriteLine($"<relativeLocation>{x},{y},{z}</relativeLocation><icon/><size/><triggerImageURL/><triggerImageWidth>0</triggerImageWidth></object><relativeAlt>0</relativeAlt><animation events=\"onCreate\">");
            Console.WriteLine($"<name>gaistart{id}</name><type>scale</type>");
            Console.WriteLine($"<length>0.25</length><delay>0</delay><interpolation>linear</interpolation><interpolationParam>0</interpolationParam><persist/><repeat/>");
            Console.WriteLine($"<from>0</from><to>0</to>");
            Console.WriteLine($"<followedBy>gaioff{id}</followedBy><axis>1,1,1</axis></animation><animation events=\"onFollow\">");
            Console.WriteLine($"<name>gaion{id}</name><type>scale</type>");
            Console.WriteLine($"<length>{length}</length><delay>0</delay><interpolation>linear</interpolation><interpolationParam>0</interpolationParam><persist/><repeat/>");
            Console.WriteLine($"<from>{size}</from><to>{size}</to>");
            Console.WriteLine($"<followedBy>gaihide{id}</followedBy><axis>1,1,1</axis></animation><animation events=\"onFollow\">");
            Console.WriteLine($"<name>gaihide{id}</name><type>scale</type>");
            Console.WriteLine($"<length>0.25</length><delay>0</delay><interpolation>linear</interpolation><interpolationParam>0</interpolationParam><persist/><repeat/>");
            Console.WriteLine($"<from>{size}</from><to>0</to>");
            Console.WriteLine($"<followedBy>gaioff{id}</followedBy><axis>1,1,1</axis></animation><animation events=\"onFollow\">");
            Console.WriteLine($"<name>gaioff{id}</name><type>scale</type>");
            Console.WriteLine($"<length>{length}</length><delay>0</delay><interpolation>linear</interpolation><interpolationParam>0</interpolationParam><persist/><repeat/> ");
            Console.WriteLine($"<from>0</from><to>0</to>");
            Console.WriteLine($"<followedBy>gaishow{id}</followedBy><axis>1,1,1</axis></animation><animation events=\"onFollow\">");
            Console.WriteLine($"<name>gaishow{id}</name><type>scale</type>");
            Console.WriteLine($"<length>0.25</length><delay>0</delay><interpolation>linear</interpolation><interpolationParam>0</interpolationParam><persist/><repeat/>");
            Console.WriteLine($"<from>0</from><to>{size}</to>");
            Console.WriteLine($"<followedBy>gaion{id}</followedBy><axis>1,1,1</axis></animation><attribution/><distance/><visibilityRange>250</visibilityRange>");
            Console.WriteLine($"<id>1000{id}</id><imageURL/>");
            Console.WriteLine($"<lat>48.158487</lat><lon>11.57869</lon><line1/><line2/><line3/><line4/>");
            Console.WriteLine($"<title>gai {id}</title><type/><doNotIndex/><showSmallBiw>1</showSmallBiw><showBiwOnClick>1</showBiwOnClick><isVisible>1</isVisible><page>poi</page></poi>");
            Console.WriteLine("");
            Console.WriteLine("");
        }

        private static int _id = 1;
        private static void PrintCircle()
        {
            PrintNaiGai(_id++, 0, 0, 0, 6, 2.5, 1.2);
            PrintNaiGai(_id++, 30, 3, 0, 5, 2.5, 1.2);
            PrintNaiGai(_id++, 60, 5, 0, 3, 2.5, 1.2);
            PrintNaiGai(_id++, 90, 6, 0, 0, 2.5, 1.2);
            PrintNaiGai(_id++, 120, 5, 0, -3, 2.5, 1.2);
            PrintNaiGai(_id++, 150, 3, 0, -5, 2.5, 1.2);
            PrintNaiGai(_id++, 180, 0, 0, -6, 2.5, 1.2);
            PrintNaiGai(_id++, -150, -3, 0, -5, 2.5, 1.2);
            PrintNaiGai(_id++, -120, -5, 0, -3, 2.5, 1.2);
            PrintNaiGai(_id++, -90, -6, 0, 0, 2.5, 1.2);
            PrintNaiGai(_id++, -60, -5, 0, 3, 2.5, 1.2);
            PrintNaiGai(_id++, -30, -3, 0, 5, 2.5, 1.2);
        }

        static void Main(string[] args)
        {
            PrintCircle();
            PrintCircle();
            PrintCircle();
            PrintCircle();
            PrintCircle();
        }
    }
}
