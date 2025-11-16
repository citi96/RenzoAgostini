<#import "base-email.ftl" as layout>
<@layout.emailLayout title=msg("updateEmailSubject")>
    <h1>${msg("updateEmailSubject")}</h1>
    <p>${msg("emailGreeting", user.firstName!user.username)}</p>
    <p>Per confermare il nuovo indirizzo email seleziona il pulsante.</p>
    <p class="card">Il link scade tra ${linkExpiration} minuti.</p>
    <p><a class="cta" href="${link}">${msg("doConfirm")}</a></p>
</@layout.emailLayout>
