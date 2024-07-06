using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly ApplicationDBContext _context;
        public PortfolioRepository(ApplicationDBContext context)
        {
            _context = context;

        }

        public async Task<Portfolio> CreateAsync(Portfolio portfolio)
        {
            try
            {


                await _context.Portfolios.AddAsync(portfolio);
                await _context.SaveChangesAsync();
                return portfolio;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Portfolio> DeleteAsync(AppUser user, string symbol)
        {
            var portfolioModel = await _context.Portfolios.FirstOrDefaultAsync(p => p.AppUserId == user.Id && p.Stock.Symbol == symbol);
            if (portfolioModel == null)
                return null;
            _context.Portfolios.Remove(portfolioModel);
            await _context.SaveChangesAsync();
            return portfolioModel;
        }

        public Task<List<Stock>> GetUserPortfolio(AppUser user)
        {
            return _context.Portfolios.Where(p => p.AppUserId == user.Id)
            .Select(stock => new Stock
            {
                Id = stock.StockId,
                Symbol = stock.Stock.Symbol,
                CompanyName = stock.Stock.CompanyName,
                Purchase = stock.Stock.Purchase,
                LastDiv = stock.Stock.LastDiv,
                Industry = stock.Stock.Industry,
                MarketCap = stock.Stock.MarketCap

            }).ToListAsync();
        }
        public async Task<bool> UserHasStock(AppUser user, string symbol)
        {
            return await _context.Portfolios
            .AnyAsync(p => p.AppUserId == user.Id && p.Stock.Symbol == symbol);
        }
    }
}