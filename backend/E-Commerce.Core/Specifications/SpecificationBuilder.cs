using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace CleanArch.Core.Specifications
{
    public static class SpecificationBuilder
    {
        public static IQueryable<T> BuildSpecification<T>(this IQueryable<T> source, Specification<T> specification)
        where T:class
        {
            if(specification.Crateria != null)
            {
                source = source.Where(specification.Crateria);
            }
            if(specification.Includes != null)
            {
                source = specification.Includes.Aggregate(source,(prev,current)=>prev.Include(current));
            }
            if(specification.Order)
            {
                
                if (specification.Desc)
                    source = source.OrderByDescending(specification.OrderBy);
                else
                    source = source.OrderBy(specification.OrderBy);
            }
            return source;
        }
    }
}
