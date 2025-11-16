<#import "base-email.ftl" as layout>
<@layout.emailLayout title=msg("linkIdpSubject")>
    <h1>${msg("linkIdpSubject")}</h1>
    <p>${msg("emailGreeting", user.firstName!user.username)}</p>
    <p>Per collegare l''account <strong>${identityProviderAlias}</strong> clicca sul pulsante.</p>
    <p><a class="cta" href="${link}">${msg("doLink")}</a></p>
</@layout.emailLayout>
