<#import "template.ftl" as layout>
<#import "user-profile.ftl" as profile>
<@layout.shell title=msg("registerTitle", msg("doRegister")) subtitle=msg("subtitleRegister")>
    <form id="kc-register-form" class="kc-form" action="${url.registrationAction}" method="post">
        <@profile.renderAttributes userProfile.attributes />
        <#if passwordRequired??>
            <div class="kc-field">
                <label for="password">${msg("password")}</label>
                <input id="password" name="password" type="password" autocomplete="new-password" />
                <@layout.fieldError fieldName="password" />
            </div>
            <div class="kc-field">
                <label for="password-confirm">${msg("passwordConfirm")}</label>
                <input id="password-confirm" name="password-confirm" type="password" autocomplete="new-password" />
            </div>
        </#if>
        <#if recaptchaRequired??>
            <div class="g-recaptcha" data-sitekey="${recaptchaSiteKey}" data-size="compact"></div>
            <script src="https://www.google.com/recaptcha/api.js" async defer></script>
        </#if>
        <div class="kc-actions">
            <div class="kc-buttons">
                <button class="kc-btn primary" type="submit">${msg("doRegister")}</button>
                <a class="kc-btn-link" href="${url.loginUrl}">${msg("backToLogin")}</a>
            </div>
        </div>
    </form>
</@layout.shell>
