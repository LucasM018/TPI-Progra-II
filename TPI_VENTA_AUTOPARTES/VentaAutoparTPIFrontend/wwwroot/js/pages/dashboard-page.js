
window.initializeDashboard = async function initializeDashboard() {

    try {

        const user = JSON.parse(storedUser);

        userName.textContent =
            `${user.nombre ?? ""} ${user.apellido ?? ""}`.trim() ||
            user.email ||
            "Usuario";

        const roles = Array.isArray(user.roles) ? user.roles : [];

        userRole.textContent = roles.length > 0
            ? roles.join(", ")
            : "Usuario autenticado";

        configureNavigation();
        configureLogout();
        configurePeriodSearch();
        configureBestSellingSearch();

        await loadDashboardData();

    } catch (error) {

        console.error("Error al inicializar el Dashboard:", error);

        if (error instanceof SyntaxError) {
            clearSession();
            window.location.replace("/pages/login.html");
        }
    }
}
