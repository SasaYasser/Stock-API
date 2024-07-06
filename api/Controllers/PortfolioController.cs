using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Extensions;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/portfolio")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        public UserManager<AppUser> _userManager { get; set; }
        public IStockRepository _stockRepo { get; set; }
        public IPortfolioRepository _portfolioRepo { get; set; }
        public PortfolioController(UserManager<AppUser> userManager, IStockRepository stockRepo, IPortfolioRepository portfolioRepo)
        {
            _userManager = userManager;
            _stockRepo = stockRepo;
            _portfolioRepo = portfolioRepo;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPortfolio()
        {
            var username = User.GetUsername();

            var appUser = await _userManager.FindByNameAsync(username);

            var userPortfolio = await _portfolioRepo.GetUserPortfolio(appUser);

            if (userPortfolio == null)
            {
                return NotFound("No portfolios were found!"); // Handle unauthorized access
            }

            return Ok(userPortfolio);

        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPortfolio(string symbol)
        {
            var username = User.GetUsername();

            var appUser = await _userManager.FindByNameAsync(username);

            var stock = await _stockRepo.GetBySymbolAsync(symbol);
            
            if (stock == null)
                return BadRequest("Stock not found!");

            if (await _portfolioRepo.UserHasStock(appUser, symbol))
            {
                return BadRequest("Can't add the same stock");
            }

            var portfolioModel = new Portfolio
            {
                StockId = stock.Id,
                AppUserId = appUser.Id
            };

            portfolioModel = await _portfolioRepo.CreateAsync(portfolioModel);

            if (portfolioModel == null)
            {
                return StatusCode(500, "Couldn't create");
            }

            return Created();
        }
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeletePortfolio(string symbol)
        {
            var username = User.GetUsername();

            var appUser = await _userManager.FindByNameAsync(username);

            var portfolioModel = await _portfolioRepo.DeleteAsync(appUser, symbol);


            if (portfolioModel == null)
            {
                return BadRequest("Stock is not found in your portfolio");

            }
            
            return Ok();

        }

    }
}