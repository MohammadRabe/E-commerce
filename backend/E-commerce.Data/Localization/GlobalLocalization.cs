using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CleanArch.Data.Localization
{
    public class GlobalLocalization
    {
        public string Localize(string textAr, string textEn)
            => CultureInfo.CurrentCulture.Name == "ar"? textAr : textEn;
    }
}
