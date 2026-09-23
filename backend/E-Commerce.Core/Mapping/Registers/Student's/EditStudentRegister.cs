using CleanArch.Core.Features.Student_s.Commands.Models;
using CleanArch.Data.Dtos;
using CleanArch.Data.Entities;
using CleanArch.Data.Localization;
using Mapster;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CleanArch.Data.Mapping.Registers
{
    public class EditStudentCommandRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            var newConfig = config
            .NewConfig<EditStudentCommand, Student>();
            if(CultureInfo.CurrentCulture.Name=="ar")
            { 
                    newConfig.Map(dest => dest.FirstNameAr, src => src.Name.Split(' ')[0])
                        .Map(dest => dest.LastNameAr, src => src.Name.Split(' ')[1]);
            }
            else
            {
                    newConfig.Map(dest => dest.FirstNameEn, src => src.Name.Split(' ')[0])
                    .Map(dest => dest.LastNameEn, src => src.Name.Split(' ')[1]);

            }
        }
    }
}
