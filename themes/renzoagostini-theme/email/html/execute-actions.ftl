<#import "base-email.ftl" as layout>
<@layout.emailLayout title=msg("executeActionsSubject")>
    <h1>${msg("executeActionsSubject")}</h1>
    <p>${msg("emailGreeting", user.firstName!user.username)}</p>
    <p>Per completare l''attivit\u00e0 richiesta esegui le azioni indicate.</p>
    <div class="card">
        <ul>
            <#list requiredActions as action>
                <li>${msg(action)}</li>
            </#list>
        </ul>
    </div>
    <p><a class="cta" href="${link}">${msg("doContinue")}</a></p>
</@layout.emailLayout>
