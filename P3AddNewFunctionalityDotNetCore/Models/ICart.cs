
using P3AddNewFunctionalityDotNetCore.Models.Entities;

namespace P3AddNewFunctionalityDotNetCore.Models
{
    public interface ICart
    {
        int GetProductQuantityInCart(Product product);
        void AddItem(Product product, int quantity);

        void RemoveLine(Product product);

        void Clear();

        double GetTotalValue();

        double GetAverageValue();

        
    }
}