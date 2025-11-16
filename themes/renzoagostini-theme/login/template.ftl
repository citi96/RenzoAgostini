<#macro shell title subtitle="">
<!DOCTYPE html>
<#-- Keycloak only exposes the locale object when internationalization is enabled. -->
<#-- Guard access so realms without locales do not trigger a FreeMarker error. -->
<#-- Default html lang to English unless locale/realm provide overrides. -->
<#if locale?? && locale.currentLanguageTag?has_content>
    <#assign htmlLang = locale.currentLanguageTag>
<#elseif realm?? && realm.defaultLocale?has_content>
    <#assign htmlLang = realm.defaultLocale>
<#else>
    <#assign htmlLang = 'en'>
</#if>
<html lang="${htmlLang}">
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
        <section class="kc-hero" aria-hidden="true">
            <div class="kc-hero-brand" aria-hidden="true">
                <img src="${url.resourcesPath}/img/logo.svg" alt="" />
            </div>
        </section>
        <section class="kc-panel">
            <header class="kc-panel-header">
                <h2>${title}</h2>
            </header>
            <#if message?has_content>
                <div class="kc-message ${message.type}">
                    ${kcSanitize(message.summary)?no_esc}
                </div>
            </#if>
            <#nested>
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
