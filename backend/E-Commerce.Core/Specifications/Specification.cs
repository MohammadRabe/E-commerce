using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CleanArch.Core.Specifications
{
    public class Specification<T>
    {
        public Specification(Expression<Func<T, bool>>? crateria, bool order,bool desc, Expression<Func<T,object>> orderBy, List<Expression<Func<T, object>>>? includes)
        {
            Crateria = crateria;
            Order = order;
            OrderBy = orderBy;
            Includes = includes;
            Desc = desc;
        }

        public Expression<Func<T,bool>>? Crateria { get; set; }
        public bool Order { get; set; }
        public bool Desc {  get; set; }
        public Expression<Func<T,object>> OrderBy { get; set; }
        public List<Expression<Func<T,object>>>? Includes { get; set; }


    }
}
