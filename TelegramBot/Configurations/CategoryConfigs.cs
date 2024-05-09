using Application.Common;
using Domain.Entities.Categories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramBot.Configurations
{
    public class CategoryConfigs
    {
        private readonly ITelegramBotClient _client;
        private readonly IGenericRepository<Category> _categoryRepository;

        public CategoryConfigs(ITelegramBotClient client, IGenericRepository<Category> categoryRepository)
        {
            _client = client;
            _categoryRepository = categoryRepository;
        }

        public async Task SendCategoriesToUser(long chatId)
        {
            var cats = await _categoryRepository.GetQuery().Select(z=> new
            {
                Name = z.Name,
                Id = z.Id
            }).ToListAsync();
            var buttons  = 
            foreach (var item in collection)
            {

            }
        }
    }
}
