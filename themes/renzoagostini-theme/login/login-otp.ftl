<#import "template.ftl" as layout>
<@layout.shell title=msg("doLogIn") subtitle=msg("subtitleOtp")>
    <form id="kc-otp-login-form" class="kc-form" action="${url.loginAction}" method="post">
        <div class="kc-field">
            <label for="otp">${msg("authenticatorCode")}</label>
            <input id="otp" name="otp" type="text" inputmode="numeric" pattern="[0-9]*" autocomplete="one-time-code" autofocus />
            <@layout.fieldError fieldName="totp" />
        </div>
        <div class="kc-description">${msg("otpLoginHint")}</div>
        <div class="kc-actions">
            <div class="kc-buttons">
                <button class="kc-btn primary" type="submit">${msg("doLogIn")}</button>
                <a class="kc-btn-link" href="${url.loginRestartFlowUrl}">${msg("backToLogin")}</a>
            </div>
        </div>
    </form>
</@layout.shell>
