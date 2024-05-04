using Application.Common;
using Application.Cqrs.Commands;
using Application.Mediator.Reminders.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Domain.DTOs.Shared;
using Domain.Entities.Reminders;

namespace Application.Mediator.Reminders.Command
{
    public class InsertReminderCommand : ICommand<ServiceRespnse>
    {
        public ReminderDTO dto { get; set; }

        public class InsertReminderCommandHandler : ICommandHandler<InsertReminderCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<Reminder> _reminderRepository;

            public InsertReminderCommandHandler(IGenericRepository<Reminder> reminderRepository)
            {
                _reminderRepository = reminderRepository;
            }

            public async Task<ServiceRespnse> Handle(InsertReminderCommand request, CancellationToken cancellationToken)
            {
                await _reminderRepository.AddAsync(new Reminder
                {
                    TelegramId = request.dto.TelegramId,
                    Amount = request.dto.Mount,
                    Description = request.dto.Description,
                    IsRemindMeAgain = request.dto.IsRemindMeAgain,
                    RemindDate = request.dto.RemindDate,
                    RemindAgainDate = request.dto.RemindAgainDate,
                });
                return new ServiceRespnse().OK();
            }
        }
    }
}
