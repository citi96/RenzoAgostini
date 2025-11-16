<#macro shell title subtitle="">
<!DOCTYPE html>
<html lang="${locale.currentLanguageTag}">
<head>
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>${title} · ${msg("appName", realm.displayName!realm.name)}</title>
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&display=swap">
    <link rel="stylesheet" href="${url.resourcesPath}/css/renzoagostini.css" />
</head>
<body class="kc-body">
    <main class="kc-shell" role="main">
        <section class="kc-hero">
            <div class="kc-brand">
                <img src="${url.resourcesPath}/img/logo.svg" alt="${msg("appName", "Renzo Agostini")}" />
                <div>
                    <strong>${msg("appName", realm.displayName!realm.name)}</strong>
                    <p>${msg("brandSubtitle", "Identità protetta")}</p>
                </div>
            </div>
            <div>
                <h1>${msg("heroTitle", "Accesso sicuro")}</h1>
                <p>${msg("heroBody", "Controlla le tue identità con un flusso essenziale")}</p>
            </div>
            <div class="kc-badge-list">
                <div class="kc-badge">
                    <strong>${msg("badgeReliability", "Affidabilità costante")}</strong>
                    <p>${msg("badgeReliabilityDesc", "Procedure lineari, nessun superfluo.")}</p>
                </div>
                <div class="kc-badge">
                    <strong>${msg("badgeSecurity", "Sicurezza attiva")}</strong>
                    <p>${msg("badgeSecurityDesc", "MFA e controlli sempre disponibili.")}</p>
                </div>
            </div>
        </section>
        <section class="kc-panel">
            <header>
                <h2>${title}</h2>
                <#if subtitle?has_content>
                    <p class="kc-description">${subtitle}</p>
                </#if>
            </header>
            <#if message?has_content>
                <div class="kc-message ${message.type}">
                    ${kcSanitize(message.summary)?no_esc}
                </div>
            </#if>
            <#nested>
            <footer class="kc-footer">
                ${msg("footerSupport", "Hai bisogno di aiuto? Contatta il supporto dedicato.")}
            </footer>
        </section>
    </main>
</body>
</html>
</#macro>

<#macro fieldError fieldName>
    <#if messagesPerField?? && messagesPerField.exists(fieldName)>
        <span class="kc-field-error">${kcSanitize(messagesPerField.get(fieldName))?no_esc}</span>
    </#if>
</#macro>
