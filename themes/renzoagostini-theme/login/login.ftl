<#import "template.ftl" as layout>
<@layout.shell title=msg("doLogIn") subtitle=msg("subtitleLogin")>
    <form id="kc-form-login" class="kc-form" onsubmit="login.disabled = true; return true;" action="${url.loginAction}" method="post">
        <input type="hidden" name="credentialId" value="${credentialId!}" />
        <div class="kc-field">
            <label for="username">${msg("usernameOrEmail", msg("username"))}</label>
            <input tabindex="1" id="username" name="username" value="${(login.username!'')?html}" type="text" autofocus autocomplete="username" />
            <@layout.fieldError fieldName="username" />
        </div>
        <#if realm.password>
            <div class="kc-field">
                <label for="password">${msg("password")}</label>
                <input tabindex="2" id="password" name="password" type="password" autocomplete="current-password" />
                <@layout.fieldError fieldName="password" />
            </div>
        </#if>
        <#if realm.rememberMe>
            <div class="kc-checkbox">
                <input tabindex="3" type="checkbox" id="rememberMe" name="rememberMe" <#if login.rememberMe?? && login.rememberMe>checked</#if> />
                <label for="rememberMe">${msg("rememberMe")}</label>
            </div>
        </#if>
        <div class="kc-actions">
            <div class="kc-buttons">
                <button tabindex="4" class="kc-btn primary" name="login" id="kc-login" type="submit">${msg("doLogIn")}</button>
                <#if realm.resetPasswordAllowed>
                    <a class="kc-btn-link" href="${url.loginResetCredentialsUrl}">${msg("doForgotPassword")}</a>
                </#if>
            </div>
            <#if realm.registrationAllowed && !registrationDisabled??>
                <p>${msg("noAccount")}
                    <a href="${url.registrationUrl}">${msg("doRegister")}</a>
                </p>
            </#if>
        </div>
    </form>
    <#if identityProviders?? && identityProviders?has_content>
        <div class="kc-social">
            <strong>${msg("identity-provider-login-label")}</strong>
            <div class="kc-social-list">
                <#list identityProviders as p>
                    <form action="${p.loginUrl}" method="post">
                        <input type="hidden" name="realm" value="${realm.name}" />
                        <button class="kc-btn secondary" type="submit">${p.displayName!p.alias}</button>
                    </form>
                </#list>
            </div>
        </div>
    </#if>
</@layout.shell>
