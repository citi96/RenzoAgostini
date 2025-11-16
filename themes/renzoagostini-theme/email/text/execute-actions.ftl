${msg("executeActionsSubject")}
${msg("emailGreeting", user.firstName!user.username)}
Azioni richieste:
<#list requiredActions as action>- ${msg(action)}
</#list>
Completa qui: ${link}
${msg("emailFooter")}
