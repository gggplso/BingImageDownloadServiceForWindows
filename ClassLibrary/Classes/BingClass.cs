using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.Classes
{
    internal class BingClass
    {
        public List<BingImage> images { get; set; }
        public BingTooltips tooltips { get; set; }
        public override string ToString()
        {
            string strTemp = string.Empty;
            foreach (var image in images)
            {
                strTemp += image.ToString() + "\n";
            }
            return string.Format($"{strTemp}\n{tooltips}");
        }
    }
    internal class BingImage
    {
        public string startdate { get; set; }
        public string fullstartdate { get; set; }
        public string enddate { get; set; }
        public string url { get; set; }
        public string urlbase { get; set; }
        public string copyright { get; set; }
        public string copyrightlink { get; set; }
        public string title { get; set; }
        public string quiz { get; set; }
        public bool wp { get; set; }
        public string hsh { get; set; }
        public int drk { get; set; }
        public int top { get; set; }
        public int bot { get; set; }
        public List<BingHS> hs { get; set; }
        public override string ToString()
        {
            string strTemp = string.Empty;
            foreach (var item in hs)
            {
                strTemp += item.ToString() + "\n";
            }
            return string.Format($"images:\n\tstartdate:{startdate}\n\tfullstartdate{fullstartdate}\n\tenddate:{enddate}\n\turl:{url}\n\turlbase:{urlbase}\n\tcopyright{copyright}\n\tcopyrightlink:{copyrightlink}\n\ttitle:{title}\n\tquiz:{quiz}\n\twp:{wp}\n\thsh:{hsh}\n\tdrk:{drk}\n\ttop:{top}\n\tbot:{bot}\n\ths:{strTemp}");
        }
    }
    internal class BingTooltips
    {
        public string loading { get; set; }
        public string previous { get; set; }
        public string next { get; set; }
        public string walle { get; set; }
        public string walls { get; set; }
        public string play { get; set; }
        public string pause { get; set; }
        public override string ToString()
        {
            return string.Format($"tooltips:\n\tloading:{loading}\n\tprevious{previous}\n\tnext{next}\n\twalle{walle}\n\twalls{walls}\n\tplay{play}\n\tpause{pause}");
        }
    }
    internal class BingHS
    {
        public override string ToString()
        {
            return string.Empty;
        }
    }
}
