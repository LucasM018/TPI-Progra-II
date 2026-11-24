window.clearSession = function clearSession() {

    sessionStorage.removeItem("accessToken");
    sessionStorage.removeItem("refreshToken");
    sessionStorage.removeItem("accessTokenExpiration");
    sessionStorage.removeItem("currentUser");

    currentAccessToken = null;
    currentRefreshToken = null;
}
