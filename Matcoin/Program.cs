using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Matcoin
{
    class Program
    {
        //core count divided by two
        static int numberOfCores = Environment.ProcessorCount / 2;

        static Block Mine(List<Block> tempBlocks, Hash.Hash hashuok, int triesAllowed, int timeAllowed)
        {
            Object lockMe = new Object();
            Console.WriteLine("Limitations - Tries: " + triesAllowed + " Time: " + timeAllowed + " ms");
            //Block minedblock = new Block();
            BlockingCollection<Block> minedblock = new BlockingCollection<Block>();
            CancellationTokenSource cts = new CancellationTokenSource();

            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = numberOfCores;
            po.CancellationToken = cts.Token;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            //guessing Hash
            
            Parallel.ForEach(tempBlocks,
                (Block temp, ParallelLoopState state) =>
            {

                for (int i = 0; i < triesAllowed; i++)
                {
                    if (state.ShouldExitCurrentIteration)
                    {
                        //state.Break();
                    }

                    if (watch.ElapsedMilliseconds > timeAllowed)
                    {
                        //state.Break();
                    }

                    temp.Nonce += 1;

                    string baseStr = temp.PrevBlockHash + temp.Date + temp.Version + temp.MerkelRootHash + temp.Nonce + temp.DifficultyTarget + temp.Nonce;

                    hashuok.Value = baseStr;

                    temp.Hash = hashuok.FingerPrint;

                    if (temp.Hash.StartsWith(temp.DifficultyTarget))
                    {
                        //lock (lockMe)
                        //{
                            //minedblock = temp;
                            minedblock.Add(temp);
                        //}
                        Console.WriteLine("Temp block that has been mined hash: " + temp.Hash);
                        state.Break();
                    }
                }
            }
           );
            


            
            if (minedblock.Count > 0)
            {
                Console.WriteLine("YOU HAVE MINED A BLOCK");
                Console.WriteLine("Blocks hash was: " + minedblock.First().Hash);
                return minedblock.First();
            }
            else
            {
                return new Block();
            }
            
            //return minedblock;


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
        static void CheckforChangedTrans(Block tempBlock)
        {
            int counter = 0;
            for (int i = 0; i < tempBlock.TransIDs.Count; i++)
            {

                bool valid = false;
                foreach (var item in tempBlock.Transactions)
                {
                    if (item.ID == tempBlock.TransIDs[i])
                    {
                        valid = true;
                    }
                }
                if (valid)
                {
                    counter++;
                }
                else
                {
                    Console.WriteLine("Transaction with real ID: " + tempBlock.TransIDs[i] + "has been changed.");
                    Console.WriteLine("Searching for fake transaction...");
                    for (int j = 0; j < tempBlock.Transactions.Count; j++)
                    {
                        bool found = false;
                        foreach (var item in tempBlock.TransIDs)
                        {
                            if (tempBlock.Transactions[i].ID == item)
                            {
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            Console.WriteLine("The fake one is: " + tempBlock.Transactions[i].ID);
                            Console.WriteLine("Removing it from the block...");
                            var removed = tempBlock.Transactions.Remove(tempBlock.Transactions[i]);
                            if (removed)
                            {
                                Console.WriteLine("Successfully removed!");
                            }
                        }
                    }
                }
            }
        }

        static Block GenerateUnfinishedBlocks(List<String> ChosenIDs, Hash.Hash hashuok, List<Transaction> Trans, List<User> Users)
        {
            //Create empty block
            Block temp = new Block();
            temp.Date = DateTime.Today.ToString();

            //create Merkel Root Hash
            List<String> Hashedtrans = new List<String>();
            temp.Transactions = new List<Transaction>();
            foreach (var item in ChosenIDs)
            {
                //checking for transaction validation

                for (int i = 0; i < Trans.Count; i++)
                {
                    bool valid = false;
                    int getter = -1;
                    int sender = -1;
                    if (Trans[int.Parse(item)].ID == Trans[i].ID)
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

                        if (Users[sender].Balance >= Trans[int.Parse(item)].value)
                        {
                            valid = true;
                        }
                    }
                    //adding trasanction
                    if (valid)
                    {
                        Hashedtrans.Add(Trans[int.Parse(item)].ID);
                        var tempas = Trans[Convert.ToInt32(item)];
                        Trans[Convert.ToInt32(item)].taken = true;
                        temp.Transactions.Add(tempas);
                    }

                }
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
            //Console.WriteLine(numberOfCores);
            //creating users
            List<User> Users;

            Hash.Hash hashuok = new Hash.Hash();
            Users = CreateUsers(hashuok);

            //creating transactions pool
            List<Transaction> Trans;
            Trans = CreateTransactions(Users, hashuok);

            List<String> ChosenIDs = new List<string>();
            //List<Block> NBlocks = new List<Block>();
            List<Block> DoneBlocks = new List<Block>();

            while (Trans.Count > 0)
            {
                //creating empty block to save the mined block into
                Block baigtas = new Block();
                //Console.WriteLine("\n\n" + Trans.Count + "\n\n");
                ChosenIDs.Clear();
                ChosenIDs = ChooseTrans(Trans);
                List<Block> tempBlocks = new List<Block>();
                bool trying = true;
                Block tempBlock = new Block();
                //Creating 6 random blocks
                for (int i = 0; i < 6; i++)
                {
                    tempBlock = GenerateUnfinishedBlocks(ChosenIDs, hashuok, Trans, Users);
                    tempBlocks.Add(tempBlock);
                }
                int timeAllowed = 5000;
                int triesAllowed = 200000;


                //trying to mine six blocks parallel
                var watch = System.Diagnostics.Stopwatch.StartNew();
                while (trying)
                {
                    tempBlock = Mine(tempBlocks, hashuok, triesAllowed, timeAllowed);
                    if (tempBlock.Hash != null)
                    {
                        trying = false;
                        baigtas = tempBlock;

                    }
                    if (trying)
                    {
                        timeAllowed += timeAllowed;
                        triesAllowed += triesAllowed;
                    }

                }

                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("Time taken for a mine: " + elapsedMs + " ms");

                //Console.WriteLine(tempBlock.Transactions.Count);
                //checking for all of the transactions validation
                CheckforChangedTrans(tempBlock);

                if (DoneBlocks.Count == 0)
                {
                    baigtas.PrevBlockHash = "00000000000000000000000000000000";
                }
                else
                {
                    baigtas.PrevBlockHash = DoneBlocks[DoneBlocks.Count - 1].Hash;
                }

                int getter = new int();

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
