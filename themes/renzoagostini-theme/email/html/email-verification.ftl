<#import "base-email.ftl" as layout>
<@layout.emailLayout title=msg("verificationSubject")>
    <h1>${msg("verificationSubject")}</h1>
    <p>${msg("emailGreeting", user.firstName!user.username)}</p>
    <p>Per confermare la tua mail su ${realm.displayName!realm.name} premi il pulsante seguente.</p>
    <p class="card">Il link resta valido per ${linkExpiration} minuti.</p>
    <p><a class="cta" href="${link}">${msg("doVerifyEmail")}</a></p>
</@layout.emailLayout>
