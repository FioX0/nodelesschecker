using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessChecker.Models
{
    public class ChainModel
    {
        public string address { get; set; }
        public string active { get; set; }
        public double difference { get; set; }
        public double users { get; set; }
        public int nodeid { get; set; }
    }
}
