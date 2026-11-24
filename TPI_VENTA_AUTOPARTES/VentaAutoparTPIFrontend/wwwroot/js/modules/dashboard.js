
window.loadDashboardData = async function loadDashboardData() {

    await Promise.allSettled([
        loadDailyBilling(),
        loadMonthlyBilling(),
        loadCriticalStock(),
        loadBestSellingAutoparts(Number(bestSellingLimit.value))
    ]);
}

window.loadDailyBilling = async function loadDailyBilling() {

    dailyBilling.textContent = "Cargando...";

    try {

        const data = await apiRequest(
            "/api/Dashboard/facturacion/diaria"
        );

        dailyBilling.textContent =
            currencyFormatter.format(data.totalFacturado);

    } catch (error) {

        console.error("Error en facturación diaria:", error);
        dailyBilling.textContent = "No disponible";
    }
}

window.loadMonthlyBilling = async function loadMonthlyBilling() {

    monthlyBilling.textContent = "Cargando...";

    try {

        const data = await apiRequest(
            "/api/Dashboard/facturacion/mensual"
        );

        monthlyBilling.textContent =
            currencyFormatter.format(data.totalFacturado);

    } catch (error) {

        console.error("Error en facturación mensual:", error);
        monthlyBilling.textContent = "No disponible";
    }
}

window.configurePeriodSearch = function configurePeriodSearch() {

    periodForm.addEventListener("submit", async event => {

        event.preventDefault();

        const initialDate = startDate.value;
        const finalDate = endDate.value;

        periodMessage.textContent = "";
        periodMessage.className = "dashboard-message";

        if (!initialDate || !finalDate) {
            showPeriodMessage(
                "Seleccioná ambas fechas para realizar la consulta.",
                "error"
            );
            return;
        }

        if (initialDate > finalDate) {
            showPeriodMessage(
                "La fecha inicial no puede ser posterior a la fecha final.",
                "error"
            );
            return;
        }

        periodButton.disabled = true;
        periodButton.innerHTML =
            '<span class="spinner-border spinner-border-sm me-2"></span>Consultando...';

        periodBilling.textContent = "Consultando...";

        try {

            const query = new URLSearchParams({
                startDate: initialDate,
                endDate: finalDate
            });

            const data = await apiRequest(
                `/api/Dashboard/facturacion/periodo?${query.toString()}`
            );

            periodBilling.textContent =
                currencyFormatter.format(data.totalFacturado);

            showPeriodMessage(
                "Consulta realizada correctamente.",
                "success"
            );

        } catch (error) {

            console.error("Error en facturación por período:", error);

            periodBilling.textContent = "No disponible";

            showPeriodMessage(error.message, "error");

        } finally {

            periodButton.disabled = false;
            periodButton.innerHTML =
                '<i class="bi bi-search me-2"></i>Consultar período';
        }
    });
}

window.showPeriodMessage = function showPeriodMessage(message, type) {

    periodMessage.textContent = message;
    periodMessage.className = `dashboard-message ${type}`;
}

window.loadCriticalStock = async function loadCriticalStock() {

    criticalStockTableBody.innerHTML = `
        <tr>
            <td colspan="5" class="table-empty">
                Cargando información...
            </td>
        </tr>
    `;

    try {

        const data = await apiRequest(
            "/api/Dashboard/stock-critico"
        );

        criticalStockTableBody.innerHTML = "";

        if (!Array.isArray(data) || data.length === 0) {

            criticalStockTableBody.innerHTML = `
                <tr>
                    <td colspan="5" class="table-empty">
                        No hay autopartes con stock crítico.
                    </td>
                </tr>
            `;

            return;
        }

        data.forEach(item => {

            const row = document.createElement("tr");

            const values = [
                item.codigo,
                item.nombre,
                item.stockActual,
                item.stockMinimo,
                item.unidadesFaltantes
            ];

            values.forEach((value, index) => {

                const cell = document.createElement("td");

                cell.textContent = value ?? "";

                if (index === 4) {
                    cell.classList.add("table-status");
                }

                row.appendChild(cell);
            });

            criticalStockTableBody.appendChild(row);
        });

    } catch (error) {

        console.error("Error al cargar stock crítico:", error);

        criticalStockTableBody.innerHTML = `
            <tr>
                <td colspan="5" class="table-empty">
                    No se pudo cargar la información del stock crítico.
                </td>
            </tr>
        `;
    }
}

window.configureBestSellingSearch = function configureBestSellingSearch() {

    bestSellingForm.addEventListener("submit", async event => {

        event.preventDefault();

        const limit = Number(bestSellingLimit.value);

        if (!Number.isInteger(limit) || limit < 1 || limit > 100) {

            alert("Ingresá una cantidad entera entre 1 y 100.");

            bestSellingLimit.focus();

            return;
        }

        bestSellingButton.disabled = true;

        bestSellingButton.innerHTML =
            '<span class="spinner-border spinner-border-sm me-2"></span>Consultando...';

        try {

            await loadBestSellingAutoparts(limit);

        } finally {

            bestSellingButton.disabled = false;

            bestSellingButton.innerHTML =
                '<i class="bi bi-search me-2"></i>Consultar ranking';
        }
    });
}

window.loadBestSellingAutoparts = async function loadBestSellingAutoparts(limit) {

    bestSellingTableBody.innerHTML = `
        <tr>
            <td colspan="3" class="table-empty">
                Cargando información...
            </td>
        </tr>
    `;

    try {

        const data = await apiRequest(
            `/api/Dashboard/autopartes-mas-vendidas?limit=${encodeURIComponent(limit)}`
        );

        bestSellingTableBody.innerHTML = "";

        if (!Array.isArray(data) || data.length === 0) {

            bestSellingTableBody.innerHTML = `
                <tr>
                    <td colspan="3" class="table-empty">
                        No hay datos de ventas disponibles.
                    </td>
                </tr>
            `;

            return;
        }

        data.forEach(item => {

            const row = document.createElement("tr");

            const values = [
                item.codigo,
                item.nombre,
                item.cantidadVendida
            ];

            values.forEach(value => {

                const cell = document.createElement("td");
                cell.textContent = value ?? "";

                row.appendChild(cell);
            });

            bestSellingTableBody.appendChild(row);
        });

    } catch (error) {

        console.error("Error al cargar autopartes más vendidas:", error);

        bestSellingTableBody.innerHTML = `
            <tr>
                <td colspan="3" class="table-empty">
                    No se pudo cargar el ranking de autopartes.
                </td>
            </tr>
        `;
    }
}


window.configureNavigation = function configureNavigation() {

    const menuItems = document.querySelectorAll("[data-section]");

    const modules = {
        dashboard: {
            title: "Dashboard",
            description: "Resumen general de ventas y stock."
        },
        autoparts: {
            title: "Autopartes",
            description: "Administración y consulta de autopartes."
        },
        invoices: {
            title: "Facturas",
            description: "Consulta y gestión de las operaciones de venta."
        },
        developers: {
            title: "Acerca de",
            description: "Conocé más sobre AutoGestock y sus desarrolladores."
        }
    };

    menuItems.forEach(item => {

        item.addEventListener("click", async event => {

            event.preventDefault();

            const section = item.dataset.section;

            if (!modules[section]) {
                return;
            }

            document.querySelectorAll(".menu-link").forEach(link => {
                link.classList.remove("active");
            });

            document.querySelectorAll(".submenu a").forEach(link => {
                link.classList.remove("active");
            });

            if (section === "dashboard") {

                item.classList.add("active");

                dashboardSection.classList.remove("d-none");
                moduleSection.classList.add("d-none");

                return;
            }

            item.classList.add("active");

            const parentMenu = item.closest(".menu-group");

            if (parentMenu) {
                parentMenu.querySelector(".menu-toggle")
                    ?.classList.add("active");
            }

            dashboardSection.classList.add("d-none");
            moduleSection.classList.remove("d-none");

            moduleTitle.textContent = modules[section].title;
            moduleDescription.textContent =
                modules[section].description;

            if (section === "autoparts") {

                await loadAutoparts();

            } else if (section === "invoices") {

                await loadInvoices();

            } else if (section === "developers") {

                renderAboutModule();
            }
        });
    });
}
