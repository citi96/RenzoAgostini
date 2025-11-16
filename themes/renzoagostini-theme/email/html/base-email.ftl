<#macro emailLayout title>
<!DOCTYPE html>
<html lang="${locale.currentLanguageTag}">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>${title}</title>
    <style>
        body { font-family: 'Inter', 'Segoe UI', sans-serif; background: #f4f4f5; color: #27272a; margin: 0; padding: 24px; }
        .container { max-width: 640px; margin: 0 auto; background: #ffffff; border-radius: 24px; padding: 32px; box-shadow: 0 20px 45px rgba(88,28,135,0.15); }
        .brand { display: flex; align-items: center; gap: 12px; margin-bottom: 24px; }
        .brand span { font-weight: 600; font-size: 1.1rem; color: #581c87; }
        h1 { font-size: 1.35rem; color: #1f2933; margin-bottom: 16px; }
        p { line-height: 1.6; margin: 0 0 16px 0; }
        .cta { display: inline-block; background: linear-gradient(135deg, #9333ea, #6b21a8); color: #fff; padding: 12px 24px; border-radius: 999px; text-decoration: none; font-weight: 600; }
        .card { border: 1px solid #e4e4e7; border-radius: 16px; padding: 20px; margin-bottom: 24px; }
        .footer { font-size: 0.85rem; color: #6b7280; margin-top: 32px; }
    </style>
</head>
<body>
    <div class="container">
        <div class="brand">
            <img src="data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' width='64' height='64' viewBox='0 0 64 64'><rect width='64' height='64' rx='18' fill='%237C3AED'/><path d='M20 44L28.5 20H34.8L43.2 44H37.5L35.6 38.5H27.8L25.8 44H20ZM29.8 33.9H33.8L31.8 27.7L29.8 33.9Z' fill='white'/></svg>" alt="Renzo Agostini" width="40" height="40" />
            <span>Renzo Agostini</span>
        </div>
        <#nested>
        <p class="footer">${msg("emailFooter")}</p>
        <p class="footer">${msg("emailSignature")}</p>
    </div>
</body>
</html>
</#macro>
