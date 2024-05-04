using Application.Common;
using Application.Cqrs.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Domain.DTOs.Shared;
using Domain.Entities.Reminders;

namespace Application.Mediator.Reminders.Command
{
    public class DeleteReminderCommand : ICommand<ServiceRespnse>
    {
        public long Id { get; set; }

        public class DeleteReminderCommandHandler : ICommandHandler<DeleteReminderCommand, ServiceRespnse>
        {
            private readonly IGenericRepository<Reminder> _reminderRepository;

            public DeleteReminderCommandHandler(IGenericRepository<Reminder> reminderRepository)
            {
                _reminderRepository = reminderRepository;
            }

            public async Task<ServiceRespnse> Handle(DeleteReminderCommand request, CancellationToken cancellationToken)
            {
                var reminder = await _reminderRepository.GetAsNoTrackingQuery().Where(z => z.Id == request.Id).FirstOrDefaultAsync();
                if (reminder is null)
                {
                    Hashtable errors = new();
                    errors.Add("Id", "Not Fount");
                    return new ServiceRespnse().Failed(System.Net.HttpStatusCode.NotFound, errors);
                }
                await _reminderRepository.SoftDeleteAsync(reminder, cancellationToken);
                return new ServiceRespnse().OK();
            }
        }
    }
}
