<#import "template.ftl" as layout>
<@layout.shell title=msg("emailForgotTitle", msg("doForgotPassword")) subtitle=msg("subtitleReset")>
    <form id="kc-reset-password-form" class="kc-form" action="${url.loginAction}" method="post">
        <div class="kc-field">
            <label for="username">${msg("emailOrUsername")}</label>
            <input type="text" id="username" name="username" value="${auth.attemptedUsername!''}" autocomplete="username" autofocus />
            <@layout.fieldError fieldName="username" />
        </div>
        <div class="kc-actions">
            <div class="kc-buttons">
                <button class="kc-btn primary" type="submit">${msg("doSubmit")}</button>
                <a class="kc-btn-link" href="${url.loginUrl}">${msg("backToLogin")}</a>
            </div>
        </div>
    </form>
</@layout.shell>
