
import "./core/state.js";
import "./core/session.js";
import "./core/utils.js";
import "./core/api.js";
import "./core/auth.js";

import "./modules/dashboard.js";
import "./modules/acerca-de.js";
import "./modules/autopartes.js";
import "./modules/facturas.js";

import "./pages/dashboard-page.js";

if (!window.currentAccessToken || !window.storedUser) {
    window.clearSession();
    window.location.replace("/pages/login.html");
} else {
    window.initializeDashboard();
}
