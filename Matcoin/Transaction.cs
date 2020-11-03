using System;
using System.Collections.Generic;
using System.Text;

namespace Matcoin
{
    public class Transaction
    {
        public string ID { get; set; }
        public string send_key { get; set; }
        public string get_key { get; set; }
        public double value { get; set; }

        public bool taken = false;
    }
}
