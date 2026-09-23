using CleanArch.Data.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;
using CleanArch.Data.Dtos;
using CleanArch.Data.Localization;

namespace CleanArch.Data.Mapping.Registers
{
    public class GetStudentByIdRegister : IRegister
    {

        public void Register(TypeAdapterConfig config)
        {
            config
                .NewConfig<Student, GetStudentByIdDto>()
                .Map(dest => dest.FullName,
                src => src.Localize(src.FirstNameAr,src.FirstNameEn) + " "+src.Localize(src.LastNameAr,src.LastNameEn))
                .Map(dest => dest.DepartmentName,
                src => src.Department.Localize(src.Department.NameAr,src.Department.NameEn));
        }
    }
}
