<#import "template.ftl" as layout>
<@layout.shell title=msg("infoTitle", msg("info")) subtitle=msg("subtitleInfo")>
    <div class="kc-description">
        <#if message?has_content>
            ${kcSanitize(message.summary)?no_esc}
        <#else>
            ${msg("subtitleInfo")}
        </#if>
    </div>
    <div class="kc-actions">
        <div class="kc-buttons">
            <a class="kc-btn primary" href="${url.loginUrl}">${msg("backToLogin")}</a>
        </div>
    </div>
</@layout.shell>
