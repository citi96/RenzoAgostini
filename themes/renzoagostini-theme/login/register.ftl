<#import "template.ftl" as layout>
<@layout.shell title=msg("registerTitle", msg("doRegister")) subtitle=msg("subtitleRegister")>
    <form id="kc-register-form" class="kc-form" action="${url.registrationAction}" method="post">
        <#if !realm.registrationEmailAsUsername>
            <div class="kc-field">
                <label for="username">${msg("username")}</label>
                <input id="username" name="username" type="text" value="${register.formData.username!''}" autocomplete="username" autofocus />
                <@layout.fieldError fieldName="username" />
            </div>
        </#if>
        <div class="kc-field">
            <label for="firstName">${msg("firstName")}</label>
            <input id="firstName" name="firstName" type="text" value="${register.formData.firstName!''}" autocomplete="given-name" />
            <@layout.fieldError fieldName="firstName" />
        </div>
        <div class="kc-field">
            <label for="lastName">${msg("lastName")}</label>
            <input id="lastName" name="lastName" type="text" value="${register.formData.lastName!''}" autocomplete="family-name" />
            <@layout.fieldError fieldName="lastName" />
        </div>
        <div class="kc-field">
            <label for="email">${msg("email")}</label>
            <input id="email" name="email" type="email" value="${register.formData.email!''}" autocomplete="email" />
            <@layout.fieldError fieldName="email" />
        </div>
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
