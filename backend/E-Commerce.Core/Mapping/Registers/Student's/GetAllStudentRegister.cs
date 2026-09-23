using CleanArch.Data.Dtos;
using CleanArch.Data.Entities;
using CleanArch.Data.Localization;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArch.Data.Mapping.Registers
{
    public class GetAllStudentRegister :IRegister
    {

        public void Register(TypeAdapterConfig config)
        {
            config
                .NewConfig<Student, GetAllStudentsDto>()
                .Map(dest => dest.FullName,
                src => src.Localize(src.FirstNameAr,src.FirstNameEn) + ' '+src.Localize(src.LastNameAr,src.LastNameEn));
        }
    }
}
