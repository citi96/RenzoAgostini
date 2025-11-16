<#import "template.ftl" as layout>
<@layout.shell title=msg("emailLinkIdpTitle", msg("email")) subtitle=msg("subtitleInfo")>
    <p class="kc-description">${msg("emailLinkIdpInstructions")}</p>
    <form action="${url.loginAction}" method="post" class="kc-form">
        <div class="kc-field">
            <label for="email">${msg("email")}</label>
            <input id="email" name="email" type="email" value="${email!''}" autocomplete="email" autofocus />
            <@layout.fieldError fieldName="email" />
        </div>
        <div class="kc-actions">
            <div class="kc-buttons">
                <button class="kc-btn primary" type="submit">${msg("doSubmit")}</button>
            </div>
        </div>
    </form>
</@layout.shell>
