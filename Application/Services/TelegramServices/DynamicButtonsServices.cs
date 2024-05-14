using Application.MapperProfile;
using AutoMapper;
using Telegram.Bot.Types.ReplyMarkups;

namespace Application.Services.TelegramServices
{

    public interface IDynamicButtonsServices
    {
        List<List<InlineKeyboardButton>> SetDynamicButtons<T>(int RowsCount, List<T> list, string CallbackData);
    }

    public class DynamicButtonsServices : IDynamicButtonsServices
    {
        private readonly IMapper _mapper;

        public DynamicButtonsServices(IMapper mapper)
        {
            _mapper = mapper;
        }

        public List<List<InlineKeyboardButton>> SetDynamicButtons<T>(int rowsCount, List<T> list, string callbackData)
        {
            var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();
            var batchCount = (int)Math.Ceiling((double)list.Count / rowsCount); 

            for (int i = 0; i < batchCount; i++)
            {
                var tempList = list.Skip(i * rowsCount).Take(rowsCount).ToList();
                var inlineButtons = new List<InlineKeyboardButton>();

                foreach (var item in tempList)
                {
                    
                    var mappedItem = _mapper.Map<NameValueDTO>(item);

                    var buttonText = mappedItem.Name.ToString();
                    var buttonCallbackData = $"{callbackData}-{mappedItem.Id}";

                    inlineButtons.Add(InlineKeyboardButton.WithCallbackData(buttonText, buttonCallbackData));
                }
               
                inlineKeyboardButtons.Add(inlineButtons);
            }

            return inlineKeyboardButtons;
        }
    }


   
}
