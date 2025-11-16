<#import "base-email.ftl" as layout>
<@layout.emailLayout title=msg("resetPasswordSubject")>
    <h1>${msg("resetPasswordSubject")}</h1>
    <p>${msg("emailGreeting", user.firstName!user.username)}</p>
    <p>Hai richiesto di reimpostare la password. Segui il link e scegli nuove credenziali.</p>
    <p class="card">Il link resta valido per ${linkExpiration} minuti.</p>
    <p><a class="cta" href="${link}">${msg("executeActionResetPassword")}</a></p>
</@layout.emailLayout>
