
window.API_BASE_URL = "https://localhost:7086";


window.userName = document.getElementById("userName");
window.userRole = document.getElementById("userRole");
window.logoutButton = document.getElementById("logoutButton");

window.dashboardSection = document.getElementById("dashboardSection");
window.moduleSection = document.getElementById("moduleSection");
window.moduleTitle = document.getElementById("moduleTitle");
window.moduleDescription = document.getElementById("moduleDescription");
window.moduleContent = document.getElementById("moduleContent");

window.dailyBilling = document.getElementById("dailyBilling");
window.monthlyBilling = document.getElementById("monthlyBilling");
window.periodBilling = document.getElementById("periodBilling");

window.periodForm = document.getElementById("periodForm");
window.startDate = document.getElementById("startDate");
window.endDate = document.getElementById("endDate");
window.periodButton = document.getElementById("periodButton");
window.periodMessage = document.getElementById("periodMessage");

window.criticalStockTableBody =
    document.getElementById("criticalStockTableBody");

window.bestSellingForm = document.getElementById("bestSellingForm");
window.bestSellingLimit = document.getElementById("bestSellingLimit");
window.bestSellingButton = document.getElementById("bestSellingButton");
window.bestSellingTableBody =
    document.getElementById("bestSellingTableBody");


window.categories = [
    {
        id: 1,
        name: "Motor",
        description: "Componentes internos y externos del motor."
    },
    {
        id: 2,
        name: "Frenos",
        description: "Componentes del sistema de frenos."
    },
    {
        id: 3,
        name: "Suspensión",
        description: "Componentes de suspensión y dirección."
    },
    {
        id: 4,
        name: "Electricidad",
        description: "Componentes eléctricos y electrónicos."
    },
    {
        id: 5,
        name: "Filtros",
        description: "Filtros de aceite, aire y combustible."
    },
    {
        id: 6,
        name: "Transmisión",
        description: "Componentes de caja y transmisión."
    }
];


window.currentAccessToken = sessionStorage.getItem("accessToken");
window.currentRefreshToken = sessionStorage.getItem("refreshToken");
window.storedUser = sessionStorage.getItem("currentUser");

window.refreshPromise = null;

window.autopartsCache = [];
window.invoicesCache = [];
window.invoiceClientsCache = [];
window.invoiceAutopartsCache = [];

window.currencyFormatter = new Intl.NumberFormat("es-AR", {
    style: "currency",
    currency: "ARS",
    minimumFractionDigits: 2
});
