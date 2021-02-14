using Basket.API.Data.Interfaces;
using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Repositories
{
    public class BasketCartRepository : IBasketCartRepository
    {
        private readonly IBasketContext _context;

        public BasketCartRepository(IBasketContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> DeleteBasket(string username)
        {
            return await _context.Redis.KeyDeleteAsync(username);
        }

        public async Task<BasketCart> GetBasket(string username)
        {
            var basket = await _context.Redis.StringGetAsync(username);
            if (basket.IsNullOrEmpty) return null;
            try
            {
                return JsonConvert.DeserializeObject<BasketCart>(basket);
            }
            catch (Exception)
            {
                return null;
            }

        }

        public async Task<BasketCart> UpdateBasket(BasketCart basketCart)
        {
            var updatedBasketCart = await _context.Redis.StringSetAsync(basketCart.Username, JsonConvert.SerializeObject(basketCart));
            if (!updatedBasketCart) return null;
            return await GetBasket(basketCart.Username);
        }
    }
}
