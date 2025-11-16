<#import "template.ftl" as layout>
<@layout.shell title=msg("updatePasswordTitle", msg("updatePassword")) subtitle=msg("subtitleUpdatePassword")>
    <form id="kc-update-password-form" class="kc-form" action="${url.loginAction}" method="post">
        <div class="kc-field">
            <label for="password-new">${msg("passwordNew")}</label>
            <input type="password" id="password-new" name="password-new" autocomplete="new-password" autofocus />
            <@layout.fieldError fieldName="password" />
        </div>
        <div class="kc-field">
            <label for="password-confirm">${msg("passwordConfirm")}</label>
            <input type="password" id="password-confirm" name="password-confirm" autocomplete="new-password" />
        </div>
        <div class="kc-actions">
            <div class="kc-buttons">
                <button class="kc-btn primary" type="submit">${msg("doSubmit")}</button>
            </div>
        </div>
    </form>
</@layout.shell>
