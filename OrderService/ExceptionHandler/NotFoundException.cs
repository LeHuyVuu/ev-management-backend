﻿namespace OrderService.ExceptionHandler
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message)
        : base(message, "NOT_FOUND") { }
    }
}
