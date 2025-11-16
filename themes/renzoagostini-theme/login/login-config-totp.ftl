<#import "template.ftl" as layout>
<@layout.shell title=msg("configureTotpTitle", msg("configureTotp")) subtitle=msg("subtitleTotp")>
    <div class="kc-description">
        ${msg("totpIntro")}
    </div>
    <#assign apps = (totp.supportedApplications?? && totp.supportedApplications?has_content)?then(totp.supportedApplications?join(', '), "Google Authenticator, Microsoft Authenticator")>
    <ol class="kc-badge-list">
        <li class="kc-badge">
            <strong>${msg("totpStep1")}</strong>
            <p>${msg("totpStep1Desc", apps)}</p>
        </li>
        <li class="kc-badge">
            <strong>${msg("totpStep2")}</strong>
            <p>${msg("totpStep2Desc")}</p>
        </li>
        <li class="kc-badge">
            <strong>${msg("totpStep3")}</strong>
            <p>${msg("totpStep3Desc")}</p>
        </li>
    </ol>
    <div class="kc-field">
        <img src="data:image/png;base64,${totpSecretQrCode}" alt="QR code" />
    </div>
    <div class="kc-field">
        <label>${msg("totpSecretLabel")}</label>
        <strong>${totpSecretEncoded}</strong>
    </div>
    <form id="kc-totp-settings-form" class="kc-form" action="${url.loginAction}" method="post">
        <div class="kc-field">
            <label for="totp">${msg("authenticatorCode")}</label>
            <input id="totp" name="totp" type="text" inputmode="numeric" pattern="[0-9]*" autocomplete="one-time-code" required />
            <@layout.fieldError fieldName="totp" />
        </div>
        <div class="kc-actions">
            <div class="kc-buttons">
                <button class="kc-btn primary" type="submit">${msg("doSubmit")}</button>
                <a class="kc-btn-link" href="${url.loginRestartFlowUrl}">${msg("backToLogin")}</a>
            </div>
        </div>
    </form>
</@layout.shell>
