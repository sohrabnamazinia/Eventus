using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    public class PremiumService
    {
        public static (int, int) GetPackage(int packageNumber)
        {
            switch (packageNumber)
            {
                case 1:
                    return (1, 15);
                case 2:
                    return (3, 40);
                case 3:
                    return (6, 75);
                case 4:
                    return (12, 140);
            }

            return (-1, -1);
        }
    }
}
