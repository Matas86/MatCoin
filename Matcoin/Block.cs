using System;
using System.Collections.Generic;
using System.Text;

namespace Matcoin
{
    public class Block
    {
        public string PrevBlockHash { get; set; }
        public string MerkelRootHash { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string DifficultyTarget { get; set; }
        public string Nonce { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
