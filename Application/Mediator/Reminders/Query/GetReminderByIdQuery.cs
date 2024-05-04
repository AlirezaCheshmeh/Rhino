using Application.Common;
using Application.Cqrs.Queris;
using Application.Mediator.Reminders.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using Domain.DTOs.Shared;
using Domain.Entities.Reminders;

namespace Application.Mediator.Reminders.Query
{
    public class GetReminderByIdQuery : IQuery<ServiceRespnse<ReminderDTO>>
    {
        public long Id { get; set; }

        public class GetReminderByIdQueryHandler : IQueryHandler<GetReminderByIdQuery, ServiceRespnse<ReminderDTO>>
        {
            private readonly IGenericRepository<Reminder> _reminderRepository;

            public GetReminderByIdQueryHandler(IGenericRepository<Reminder> reminderRepository)
            {
                _reminderRepository = reminderRepository;
            }

            public async Task<ServiceRespnse<ReminderDTO>> Handle(GetReminderByIdQuery request, CancellationToken cancellationToken)
            {
                var reminder = await _reminderRepository.GetAsNoTrackingQuery().Where(z => z.Id == request.Id).Select(z => new ReminderDTO
                {
                    Description = z.Description,
                    TelegramId = z.TelegramId,
                    Id = z.Id,
                    IsRemindMeAgain = z.IsRemindMeAgain,
                    Mount = z.Amount,
                    RemindAgainDate = z.RemindAgainDate,
                    RemindDate = z.RemindDate
                }).FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if (reminder is not null) return new ServiceRespnse<ReminderDTO>().OK(reminder);
                Hashtable errors = new();
                errors.Add("Id", "Not Fount");
                return new ServiceRespnse<ReminderDTO>().Failed(System.Net.HttpStatusCode.NotFound, errors);
            }
        }
    }
}
