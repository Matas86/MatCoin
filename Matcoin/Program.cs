using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;

namespace Matcoin
{
    class Program
    {
       
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
        static List<Transaction> CreateTransactions(List<User> Users,  Hash.Hash hashuok)
        {
            List<Transaction> Trans = new List<Transaction>();
            Random rnd = new Random();
            for (int i = 0; i < 10000; i++)
            {
                Transaction temp = new Transaction();
                int first = rnd.Next(0, 499);
                int second = rnd.Next(500,999);
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
        
        static List<String> ChooseTrans(List<Transaction> Trans, Hash.Hash hashuok)
        {
            List<String> IDs = new List<string>();
            Random rnd = new Random();
            for (int i = 0; i < 100; i++)
            {
                int count = 1;
                int number = -1;
                while(count > 0)
                {
                    count = 0;
                    number = rnd.Next(0, 9999);
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
            return IDs;
        }

        static Block GenerateUnfinishedBlocks(List<String> ChosenIDs, Hash.Hash hashuok, List<Transaction> Trans)
        {
            Block temp = new Block();
            while(ChosenIDs.Count > 1)
            {
                List<String> Hashedtrans = new List<String>();
                for (int i = 0; i < ChosenIDs.Count; i=+2)
                {
                    
                    string tempString = Trans[int.Parse(ChosenIDs[i])].ID + Trans[int.Parse(ChosenIDs[i+1])].ID;
                    hashuok.Value = tempString;
                    Hashedtrans.Add(hashuok.FingerPrint);
                    
                }
                ChosenIDs = Hashedtrans;
            }
            Console.WriteLine(ChosenIDs.Count);
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
            Trans = CreateTransactions(Users,  hashuok);

            List<String> ChosenIDs = new List<string>();
            List<Block> NBlocks = new List<Block>();

            for (int i = 0; i < 100; i++)
            {
                ChosenIDs.Clear();
                ChosenIDs = ChooseTrans(Trans, hashuok);
                NBlocks.Add(GenerateUnfinishedBlocks(ChosenIDs, hashuok, Trans));
            }
            
        }
    }
}
