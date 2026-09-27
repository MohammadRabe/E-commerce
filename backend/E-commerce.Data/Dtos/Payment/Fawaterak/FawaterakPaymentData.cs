using System;
using System.Collections.Generic;
using System.Text;

namespace E_commerce.Data.Dtos.Payment.Fawaterak
{
    public sealed record FawaterakPaymentData(
    string? RedirectTo,
    string? FawryCode,
    string? AmanCode,
    string? MasaryCode,
    string? MeezaReference);
}
