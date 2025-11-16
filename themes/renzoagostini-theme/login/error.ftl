<#import "template.ftl" as layout>
<@layout.shell title=msg("errorTitle", msg("errorTitle")) subtitle=msg("subtitleError")>
    <div class="kc-message error">
        <#if message?has_content>
            ${kcSanitize(message.summary)?no_esc}
        <#else>
            ${msg("unexpectedErrorMessage")}
        </#if>
    </div>
    <div class="kc-actions">
        <div class="kc-buttons">
            <a class="kc-btn primary" href="${url.loginUrl}">${msg("backToLogin")}</a>
        </div>
    </div>
</@layout.shell>
