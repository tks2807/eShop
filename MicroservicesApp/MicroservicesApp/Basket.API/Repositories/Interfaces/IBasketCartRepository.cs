using Basket.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Repositories.Interfaces
{
    public interface IBasketCartRepository
    {
        Task<BasketCart> GetBasket(string username);
        Task<BasketCart> UpdateBasket(BasketCart basketCart);
        Task<bool> DeleteBasket(string username);
    }
}
