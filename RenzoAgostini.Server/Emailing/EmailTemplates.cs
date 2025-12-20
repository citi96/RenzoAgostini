
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
            <p>Ciao {name},</p>
            <p>Grazie per esserti registrato alla Galleria Renzo Agostini.</p>
            <p>Siamo lieti di averti con noi. Potrai ora esplorare la nostra collezione completa, richiedere opere personalizzate e gestire i tuoi ordini.</p>
            <p>Se hai domande o desideri maggiori informazioni, non esitare a contattarci.</p>
            <br>
            <p>A presto,<br>Il team della Galleria Agostini</p>";

        return BaseTemplate("Benvenuto!", content);
    }

    public static string GetResetPasswordEmail(string name, string resetLink)
    {
        var content = $@"
            <p>Ciao {name},</p>
            <p>Abbiamo ricevuto una richiesta di reimpostazione della password per il tuo account.</p>
            <p>Per procedere, clicca sul pulsante qui sotto:</p>
            <div style=""text-align: center; margin: 30px 0;"">
                <a href=""{resetLink}"" style=""{ButtonStyle}"">Reimposta Password</a>
            </div>
            <p>Se non hai richiesto tu la reimpostazione, puoi ignorare questa email in sicurezza.</p>";

        return BaseTemplate("Reimposta la tua password", content);
    }
}
