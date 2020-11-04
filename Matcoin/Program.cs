using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Xml.Schema;

namespace Matcoin
{
    class Program
    {

        static Block Mine(Block temp, Hash.Hash hashuok)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //guessing Hash
            while (!temp.Hash.StartsWith(temp.DifficultyTarget))
            {
                temp.Nonce += 1;
                string baseStr = temp.PrevBlockHash + temp.Date + temp.Version + temp.MerkelRootHash + temp.Nonce + temp.DifficultyTarget + temp.Nonce;
                hashuok.Value = baseStr;
                temp.Hash = hashuok.FingerPrint;
            }
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("YOU HAVE MINED A BLOCK");
            Console.WriteLine("Blocks hash was: " + temp.Hash);
            Console.WriteLine("The time mining took: " + elapsedMs + " ms");
            return temp;

        }
        static string BuildMerkleRoot(List<String> merkelLeaves, Hash.Hash hashuok)
        {
            if (merkelLeaves == null || !merkelLeaves.Any())
            {
                return string.Empty;
            }
            if (merkelLeaves.Count() == 1)
            {
                return merkelLeaves.First();
            }
            if (merkelLeaves.Count() % 2 > 0)
            {
                merkelLeaves.Add(merkelLeaves.Last());
            }
            var merkleBranches = new List<String>();
            for (int i = 0; i < merkelLeaves.Count; i += 2)
            {
                var leafPair = string.Concat(merkelLeaves[i], merkelLeaves[i + 1]);
                hashuok.Value = leafPair;
                merkleBranches.Add(hashuok.FingerPrint);
            }
            return BuildMerkleRoot(merkleBranches, hashuok);
        }

        static List<User> CreateUsers(Hash.Hash hashuok)
        {
            List<User> Users = new List<User>();
            Random rnd = new Random();
            for (int i = 0; i < 1000; i++)
            {
                User temp = new User();
                temp.Name = "U" + i;
                hashuok.Value = temp.Name;
                temp.PublicKey = hashuok.FingerPrint;
                temp.Balance = rnd.Next(100, 1000000);
                Users.Add(temp);
            }
            return Users;
        }
        static List<Transaction> CreateTransactions(List<User> Users, Hash.Hash hashuok)
        {
            List<Transaction> Trans = new List<Transaction>();
            Random rnd = new Random();
            for (int i = 0; i < 10000; i++)
            {
                Transaction temp = new Transaction();
                int first = rnd.Next(0, 499);
                int second = rnd.Next(500, 999);
                temp.send_key = Users[first].PublicKey;
                temp.get_key = Users[second].PublicKey;
                temp.value = rnd.Next(1, 1000);
                string addAll = temp.send_key + temp.get_key + temp.value.ToString();
                hashuok.Value = addAll;
                temp.ID = hashuok.FingerPrint;
                Trans.Add(temp);
            }
            return Trans;
        }

        static List<String> ChooseTrans(List<Transaction> Trans)
        {
            List<String> IDs = new List<string>();
            Random rnd = new Random();
            if (Trans.Count > 100)
            {
                for (int i = 0; i < 100; i++)
                {
                    int count = 1;
                    int number = -1;
                    while (count > 0)
                    {
                        count = 0;
                        number = rnd.Next(0, Trans.Count);
                        foreach (var item in IDs)
                        {
                            if (int.Parse(item) == number)
                            {
                                count++;
                            }
                        }
                    }
                    Trans[number].taken = true;
                    IDs.Add(number.ToString());

                }
            }
            else
            {
                for (int i = 0; i < Trans.Count; i++)
                {
                    IDs.Add(i.ToString());
                }
            }
            
            return IDs;
        }

        static Block GenerateUnfinishedBlocks(List<String> ChosenIDs, Hash.Hash hashuok, List<Transaction> Trans, List<Block> NBlocks)
        {
            //Create empty block
            Block temp = new Block();
            temp.Date = DateTime.Today.ToString();

            //create Merkel Root Hash
            List<String> Hashedtrans = new List<String>();
            temp.Transactions = new List<Transaction>();
            foreach (var item in ChosenIDs)
            {
                Hashedtrans.Add(Trans[int.Parse(item)].ID);
                var tempas = Trans[Convert.ToInt32(item)];
                Trans[Convert.ToInt32(item)].taken = true;
                temp.Transactions.Add(tempas);
            }
            temp.TransIDs = Hashedtrans;
            temp.MerkelRootHash = BuildMerkleRoot(Hashedtrans, hashuok);

            temp.PrevBlockHash = "";
            //setting other parameters of header
            temp.Nonce = 0;
            temp.DifficultyTarget = "00000";
            temp.Version = "1.0";
            temp.Hash = "";


            return temp;
        }
        static void Main(string[] args)
        {
            //creating users
            List<User> Users;

            Hash.Hash hashuok = new Hash.Hash();
            Users = CreateUsers(hashuok);

            //creating transactions pool
            List<Transaction> Trans;
            Trans = CreateTransactions(Users, hashuok);

            List<String> ChosenIDs = new List<string>();
            List<Block> NBlocks = new List<Block>();
            List<Block> DoneBlocks = new List<Block>();

            while(Trans.Count > 0)
            {
                //Console.WriteLine("\n\n" + Trans.Count + "\n\n");
                ChosenIDs.Clear();
                ChosenIDs = ChooseTrans(Trans);
                var tempBlock = GenerateUnfinishedBlocks(ChosenIDs, hashuok, Trans, NBlocks);

                var baigtas = Mine(tempBlock, hashuok);
                if (DoneBlocks.Count == 0)
                {
                    baigtas.PrevBlockHash = "00000000000000000000000000000000";
                }
                else
                {
                    baigtas.PrevBlockHash = DoneBlocks[DoneBlocks.Count - 1].Hash;
                }
                int getter = -1;
                int sender = -1;
                foreach (var tran in baigtas.TransIDs)
                {
                    for (int i = 0; i < Trans.Count; i++)
                    {
                        //find transaction in the pool
                        if (tran == Trans[i].ID)
                        {
                            //found sender and getter of the transaction
                            for (int j = 0; j < Users.Count; j++)
                            {
                                if (Trans[i].get_key == Users[j].PublicKey)
                                {
                                    getter = j;
                                    if (sender != -1)
                                    {
                                        break;
                                    }
                                }
                                if (Trans[i].send_key == Users[j].PublicKey)
                                {
                                    sender = j;
                                    if (getter != -1)
                                    {
                                        break;
                                    }
                                }
                            }
                            //check for transaction validation
                            /*Console.WriteLine("The balance of the sender before transaction: " + Users[sender].Balance);
                            Console.WriteLine("The balance of the getter before transaction: " + Users[getter].Balance);
                            Users[getter].Balance += Trans[i].value;
                            Users[sender].Balance -= Trans[i].value;
                            Console.WriteLine("The balance of the sender after transaction: " + Users[sender].Balance);
                            Console.WriteLine("The balance of the getter after transaction: " + Users[getter].Balance);*/
                            //delete transaction from the pool
                            Trans.RemoveAt(i);
                            break;
                        }
                    }
                DoneBlocks.Add(baigtas);
            }
        }

    }
}
}
