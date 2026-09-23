using CleanArch.Core.Features.Student_s.Commands.Models;
using CleanArch.Data.Dtos;
using CleanArch.Data.Entities;
using CleanArch.Data.Localization;
using CleanArch.Data.Mapping.Registers;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Core.Mapping.Registers
{
    public class GetStudentsPagenatedRegister : IRegister
    {


        public void Register(TypeAdapterConfig config)
        {
            config
                .NewConfig<Student, GetStudentsPagenatedDto>()
                .Map(dest => dest.FullName
                , src => src.Localize(src.FirstNameAr,src.FirstNameEn) + " " + src.Localize(src.LastNameAr,src.LastNameEn))
                .Map(dest => dest.DepartmentName, src => src.Localize(src.Department.NameAr,src.Department.NameEn));
        }
    }
}
