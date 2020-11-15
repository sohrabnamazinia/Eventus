using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ArsamBackend.Models
{
    [Flags]
    public enum Category
    {
        Race = 1,
        Performance = 2,
        Conference = 4,
        Fundraiser = 8,
        Festival = 16,
        SocialEvent = 32
    }

    public class CategoryService
    {
        public static List<int> ConvertCategoriesToList(Category category)
        {
            List<int> result = new List<int>();
            foreach (Category cat in Enum.GetValues(typeof(Category)))
                if ((category & cat) != 0)
                    result.Add((int)cat);

            return result;
        }

        public static Category BitWiseOr(List<int> categories)
        {
            Category result = 0;
            foreach (var i in categories)
                result |= (Category)i;

            return result;
        }

        public static bool FilterCategory(List<int> FilteredCategories, Category category)
        {
            List<int> EventCategories = ConvertCategoriesToList(category);
            bool IsSubset = !FilteredCategories.Except(EventCategories).Any();
            return IsSubset;
        }
    }
}
