<#import "base-email.ftl" as layout>
<@layout.emailLayout title=msg("updatePasswordSubject")>
    <h1>${msg("updatePasswordSubject")}</h1>
    <p>${msg("emailGreeting", user.firstName!user.username)}</p>
    <p>La tua password \u00e8 stata aggiornata correttamente. Se non riconosci l''operazione esegui subito un nuovo cambio dalle impostazioni.</p>
</@layout.emailLayout>
