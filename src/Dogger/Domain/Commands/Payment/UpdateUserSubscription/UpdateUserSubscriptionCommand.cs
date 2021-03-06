﻿using System;
using MediatR;

namespace Dogger.Domain.Commands.Payment.UpdateUserSubscription
{
    public class UpdateUserSubscriptionCommand : IRequest
    {
        public Guid UserId { get; }

        public UpdateUserSubscriptionCommand(
            Guid userId)
        {
            this.UserId = userId;
        }
    }
}
