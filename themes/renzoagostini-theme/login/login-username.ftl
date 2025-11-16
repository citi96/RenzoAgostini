<#import "template.ftl" as layout>
<@layout.shell title=msg("usernamePasswordFormTitle", msg("username")) subtitle=msg("subtitleLogin")>
    <form id="kc-username-form" class="kc-form" action="${url.loginAction}" method="post">
        <div class="kc-field">
            <label for="username">${msg("usernameOrEmail", msg("username"))}</label>
            <input id="username" name="username" type="text" value="${username!''}" autocomplete="username" autofocus />
            <@layout.fieldError fieldName="username" />
        </div>
        <div class="kc-actions">
            <div class="kc-buttons">
                <button class="kc-btn primary" type="submit">${msg("doContinue")}</button>
                <a class="kc-btn-link" href="${url.loginUrl}">${msg("backToLogin")}</a>
            </div>
        </div>
    </form>
</@layout.shell>
