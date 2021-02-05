using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public static class Balance
    {
        
        public static Tuple Charge(int amount, int balance)
        {
            Tuple tuple = new Tuple();
            
            if (balance < 0)
            {
                tuple.error = "balance is negative";
                tuple.result = -1;
                return tuple;
            }
            else if (amount <= 0)
            {
                tuple.result = -1;
                tuple.error = "charge amount must be grater than zero";
            }
            tuple.result = balance + amount;
            return tuple;
        }
        public static Tuple Pay(int amount, int balance)
        {
            Tuple tuple = new Tuple();
            int newBalance = balance - amount;

            if (balance < 0)
            {
                tuple.error = "balance is negative";
                tuple.result = -1;
                return tuple;
            }
            else if (amount <= 0)
            {
                tuple.result = -1;
                tuple.error = "charge amount must be grater than zero";
                return tuple;
            }

            else if (newBalance < 0)
            {
                tuple.result = -1;
                tuple.error = "not enough balance!";
                return tuple;
            }

            tuple.result = newBalance;
            return tuple;
        }

    }

    public class Tuple
    {
        public int result;
        public string error = null;
    }
}
