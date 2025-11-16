<#import "template.ftl" as layout>
<#import "user-profile.ftl" as profile>
<@layout.shell title=msg("updateProfileTitle", msg("updateProfile")) subtitle=msg("subtitleUpdateProfile")>
    <form id="kc-update-profile-form" class="kc-form" action="${url.loginAction}" method="post">
        <@profile.renderAttributes userProfile.attributes />
        <div class="kc-actions">
            <div class="kc-buttons">
                <button class="kc-btn primary" type="submit">${msg("doSubmit")}</button>
            </div>
        </div>
    </form>
</@layout.shell>
