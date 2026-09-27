using System;
using System.Collections.Generic;
using System.Text;

namespace E_commerce.Data.Dtos.Payment.Fawaterak
{

    public sealed record FawaterakTransactionDataResultModel(
        int OrderId,
        string IntentKey,
        string Url,
        int ExpiresIn);

}
