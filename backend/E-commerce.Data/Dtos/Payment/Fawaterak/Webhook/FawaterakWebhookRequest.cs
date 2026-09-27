using System;
using System.Collections.Generic;
using System.Text;

namespace E_commerce.Data.Dtos.Payment.Fawaterak.Webhook
{
    public sealed record FawaterakWebhookRequest(
    string hashKey,
    int invoice_id,
    string invoice_key,
    string payment_method,
    string invoice_status,
    object? pay_load,
    string referenceNumber);
}

