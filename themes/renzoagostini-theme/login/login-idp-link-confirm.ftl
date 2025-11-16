<#import "template.ftl" as layout>
<@layout.shell title=msg("confirmLinkIdpTitle") subtitle=msg("subtitleInfo")>
    <p class="kc-description">${msg("confirmLinkIdpReviewProfile")}</p>
    <div class="kc-actions">
        <div class="kc-buttons">
            <form action="${url.loginAction}" method="post">
                <button class="kc-btn primary" type="submit" name="submitAction" value="link">${msg("doLink")}</button>
                <button class="kc-btn secondary" type="submit" name="submitAction" value="cancel">${msg("doCancel")}</button>
            </form>
        </div>
    </div>
</@layout.shell>
