
window.configureLogout = function configureLogout() {

    logoutButton.addEventListener("click", async () => {

        logoutButton.disabled = true;

        try {

            const token = sessionStorage.getItem("refreshToken");

            if (token) {

                await fetch(
                    `${API_BASE_URL}/api/Auth/logout`,
                    {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json"
                        },
                        body: JSON.stringify({
                            refreshToken: token
                        })
                    }
                );
            }

        } catch (error) {

            console.error("Error al cerrar sesión:", error);

        } finally {

            clearSession();

            window.location.replace("/pages/login.html");
        }
    });
}
