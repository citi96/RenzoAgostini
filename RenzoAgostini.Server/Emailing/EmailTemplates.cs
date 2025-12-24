
namespace RenzoAgostini.Server.Emailing;

public static class EmailTemplates
{
    private const string ContainerStyle = "font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 40px 20px; color: #333; line-height: 1.6;";
    private const string HeaderStyle = "text-align: center; margin-bottom: 40px;";
    private const string LogoStyle = "max-height: 50px; margin-bottom: 20px;"; // Assuming a logo exists, or use text
    private const string TitleStyle = "font-size: 24px; font-weight: 300; margin-bottom: 20px; color: #111;";
    private const string BodyStyle = "font-size: 16px; margin-bottom: 30px; color: #555;";
    private const string ButtonStyle = "display: inline-block; padding: 12px 24px; background-color: #111; color: #fff; text-decoration: none; border-radius: 4px; font-weight: 500; font-size: 14px;";
    private const string FooterStyle = "margin-top: 40px; padding-top: 20px; border-top: 1px solid #eee; text-align: center; font-size: 12px; color: #999;";

    private static string BaseTemplate(string title, string content)
    {
        return $@"
            <div style=""{ContainerStyle}"">
                <div style=""{HeaderStyle}"">
                    <h1 style=""{TitleStyle}"">Galleria Agostini</h1> 
                </div>
                <div style=""{BodyStyle}"">
                    <h2 style=""font-size: 20px; font-weight: 400; margin-bottom: 15px;"">{title}</h2>
                    {content}
                </div>
                <div style=""{FooterStyle}"">
                    &copy; {DateTime.Now.Year} Galleria Renzo Agostini. Tutti i diritti riservati.<br>
                    Questa è una mail automatica, per favore non rispondere.
                </div>
            </div>";
    }

    public static string GetWelcomeEmail(string name)
    {
        var content = $@"
            <p>Salve {name},</p>
            <p>Grazie per esserti registrato alla Galleria Renzo Agostini.</p>
            <p>Sono lieto di averti con me. Potrai ora esplorare la mia collezione completa, richiedere opere personalizzate e gestire i tuoi ordini.</p>
            <p>Se hai domande o desideri maggiori informazioni, non esitare a contattarmi.</p>
            <br>
            <p>A presto,<br>Renzo Agostini</p>";

        return BaseTemplate("Benvenuto!", content);
    }

    public static string GetResetPasswordEmail(string name, string resetLink)
    {
        var content = $@"
            <p>Salve {name},</p>
            <p>Ho ricevuto una richiesta di reimpostazione della password per il tuo account.</p>
            <p>Per procedere, clicca sul pulsante qui sotto:</p>
            <div style=""text-align: center; margin: 30px 0;"">
                <a href=""{resetLink}"" style=""{ButtonStyle}"">Reimposta Password</a>
            </div>
            <p>Se non hai richiesto tu la reimpostazione, puoi ignorare questa email in sicurezza.</p>";

        return BaseTemplate("Reimposta la tua password", content);
    }
    public static string GetOrderPaidEmail(string orderNumber, string customerName, string orderDate, string orderTotal, string paymentMethod, string orderUrl, string supportUrl)
    {
        var logoUrl = "https://www.renzoagostini.it/assets/logo.png";
        var brandName = "Galleria Renzo Agostini";
        // var brandAddress = "Via dell'Arte, 1, Roma"; // Placeholder or Config

        return $@"
<!doctype html>
<html lang=""it"">
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width"">
    <title>Conferma ordine {orderNumber}</title>
    <style>
        body, table, td, a {{ -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; }}
        table, td {{ mso-table-lspace: 0pt; mso-table-rspace: 0pt; }}
        img {{ -ms-interpolation-mode: bicubic; border: 0; line-height: 100%; outline: none; text-decoration: none; }}
        table {{ border-collapse: collapse !important; }}
        body {{ margin: 0; padding: 0; width: 100% !important; }}
    </style>
</head>
<body style=""background:#f4f6f8; margin:0; padding:0;"">
    <div style=""display:none; max-height:0; overflow:hidden;"">Ordine n. {orderNumber} ricevuto con successo.</div>
    <table role=""presentation"" width=""100%"" bgcolor=""#f4f6f8"">
        <tr>
            <td align=""center"" style=""padding:24px;"">
                <table role=""presentation"" width=""600"" style=""background:#ffffff; border-radius:8px; overflow:hidden;"">
                    <tr>
                        <td align=""left"" style=""padding:24px 24px 0 24px; font-family:Arial, Helvetica, sans-serif;"">
                            <img src=""{logoUrl}"" alt=""{brandName}"" width=""120"" style=""display:block;"">
                        </td>
                    </tr>
                    <tr>
                        <td align=""left"" style=""padding:8px 24px 0 24px; font-family:Arial, Helvetica, sans-serif;"">
                            <h1 style=""margin:0; font-size:22px; line-height:1.3; color:#111827;"">
                                Ordine n. <span style=""white-space:nowrap;"">{orderNumber}</span> ricevuto con successo
                            </h1>
                            <p style=""margin:8px 0 0 0; font-size:14px; color:#374151;"">
                                Salve {customerName}, ho ricevuto il tuo ordine.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td align=""left"" style=""padding:16px 24px; font-family:Arial, Helvetica, sans-serif;"">
                            <table role=""presentation"" width=""100%"" style=""border:1px solid #e5e7eb; border-radius:6px;"">
                                <tr>
                                    <td style=""padding:12px 16px; font-size:14px; color:#111827;"">Data</td>
                                    <td align=""right"" style=""padding:12px 16px; font-size:14px; color:#111827;"">{orderDate}</td>
                                </tr>
                                <tr>
                                    <td style=""padding:12px 16px; font-size:14px; color:#111827;"">Totale</td>
                                    <td align=""right"" style=""padding:12px 16px; font-size:14px; color:#111827;"">{orderTotal}</td>
                                </tr>
                                <tr>
                                    <td style=""padding:12px 16px; font-size:14px; color:#111827;"">Metodo di pagamento</td>
                                    <td align=""right"" style=""padding:12px 16px; font-size:14px; color:#111827;"">{paymentMethod}</td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td align=""left"" style=""padding:0 24px 8px 24px; font-family:Arial, Helvetica, sans-serif;"">
                            <p style=""margin:0; font-size:14px; color:#374151;"">
                                Riceverai un’altra email quando l’ordine sarà spedito.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td align=""left"" style=""padding:0 24px 16px 24px; font-family:Arial, Helvetica, sans-serif;"">
                            <a href=""{orderUrl}"" style=""display:inline-block; background:#111827; color:#ffffff; text-decoration:none; padding:10px 16px; border-radius:6px; font-size:14px;"">
                                Vedi ordine {orderNumber}
                            </a>
                        </td>
                    </tr>
                    <tr>
                        <td align=""left"" style=""padding:16px 24px 24px 24px; font-family:Arial, Helvetica, sans-serif; font-size:12px; color:#6b7280; border-top:1px solid #e5e7eb;"">
                            {brandName}<br>
                            Domande? <a href=""{supportUrl}"" style=""color:#111827; text-decoration:underline;"">Supporto</a>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
