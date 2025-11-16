<#import "template.ftl" as layout>
<#import "user-profile.ftl" as profile>
<@layout.shell title=msg("confirmLinkIdpTitle") subtitle=msg("subtitleUpdateProfile")>
    <form id="kc-review-profile" class="kc-form" action="${url.loginAction}" method="post">
        <@profile.renderAttributes userProfile.attributes />
        <div class="kc-actions">
            <div class="kc-buttons">
                <button class="kc-btn primary" type="submit" name="submitAction" value="link">${msg("doSubmit")}</button>
                <button class="kc-btn secondary" type="submit" name="submitAction" value="cancel">${msg("doCancel")}</button>
            </div>
        </div>
    </form>
</@layout.shell>
