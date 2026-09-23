using E_commerce.Infrastructure.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace E_commerce.Service.Services
{
    public abstract class BaseService
    {
        protected IUOW _uow;

        protected BaseService(IUOW uow)
        {
            _uow = uow;
        }
    }
}
