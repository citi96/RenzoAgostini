let authenticationStateProviderInstance = null;

function googleInitialize(clientId, authenticationStateProvider) {
    // disable Exponential cool-down
    /*document.cookie = `g_state=;path=/;expires=Thu, 01 Jan 1970 00:00:01 GMT`;*/
    authenticationStateProviderInstance = authenticationStateProvider;
    google.accounts.id.initialize({ client_id: clientId, callback: callback });
}

function googlePrompt() {
    google.accounts.id.prompt((notification) => {
        if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
            console.info(notification.getNotDisplayedReason());
            console.info(notification.getSkippedReason());
        }
    });
}

function callback(googleResponse) {
    authenticationStateProviderInstance.invokeMethodAsync("GoogleLogin", {
        ClientId: googleResponse.clientId,
        SelectedBy: googleResponse.select_by,
        Credential: googleResponse.credential
    });
}