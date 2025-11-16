<#import "template.ftl" as layout>
<@layout.shell title=msg("errorTitle", msg("errorTitle")) subtitle=msg("subtitleError")>
    <section class="kc-error-stack">
        <p class="kc-error-intro">${msg("errorIntro", "Our monitoring detected an unexpected interruption.")}</p>
        <div class="kc-error-card primary">
            <span class="kc-error-label">${msg("errorCardPrimaryLabel", "What happened")}</span>
            <p>
                <#if message?has_content>
                    ${kcSanitize(message.summary)?no_esc}
                <#else>
                    ${msg("unexpectedErrorMessage")}
                </#if>
            </p>
        </div>
        <div class="kc-error-card secondary">
            <span class="kc-error-label">${msg("errorCardSecondaryLabel", "Automatic diagnostics")}</span>
            <p>${msg("unexpectedErrorMessage")}</p>
        </div>
    </section>
    <div class="kc-actions">
        <div class="kc-buttons">
            <a class="kc-btn primary" href="${url.loginUrl}">${msg("backToLogin")}</a>
        </div>
        <p class="kc-error-support">${msg("errorSupport", "If the issue persists contact your technical referent.")}</p>
    </div>
</@layout.shell>
