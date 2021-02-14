using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Basket.API.Entities;
using Basket.API.Repositories.Interfaces;
using EventBusRabbitMQ.Common;
using EventBusRabbitMQ.Events;
using EventBusRabbitMQ.Producers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace Basket.API.Controllers
{
    [Route("api/v1/Basket")]
    [ApiController]
    public class BasketCartController : ControllerBase
    {
        private readonly IBasketCartRepository _repository;
        private readonly ILogger<BasketCartController> _logger;
        private readonly EventBusRabbitMQProducer _eventBus;
        private readonly IMapper _mapper;

        public BasketCartController(IBasketCartRepository repository, ILogger<BasketCartController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BasketCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<BasketCart>> GetBasket(string userName)
        {
            var basket = await _repository.GetBasket(userName);
            return Ok(basket ?? new BasketCart(userName));
        }

        [HttpPost]
        [ProducesResponseType(typeof(BasketCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<BasketCart>> UpdateBasket([FromBody] BasketCart basket)
        {
            return Ok(await _repository.UpdateBasket(basket));
        }

        [HttpDelete("{userName}")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBasket(string username)
        {
            return Ok(await _repository.DeleteBasket(username));
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            var basket = await _repository.GetBasket(basketCheckout.UserName);
            if(basket == null)
            {
                //_logger.LogError("Basket could not be find for user: " + basketCheckout.UserName);
                return BadRequest();
            }

            var removed = await _repository.DeleteBasket(basketCheckout.UserName);
            if (!removed)
            {
                //_logger.LogError("Basket could not be removed for user: " + basketCheckout.UserName);
                return BadRequest();
            }

            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout.UserName);
            eventMessage.RequestId = Guid.NewGuid();

            try
            {
                _eventBus.PublishBasketCheckout(EventBusConstants.BasketCheckoutQueue, eventMessage);
            } catch (Exception e)
            {
                //_logger.LogError("Error: Message cannot be published for user: " + basketCheckout.UserName + ", " + e.Message);
                throw;
            }
            return Accepted();
        }
    }
}
