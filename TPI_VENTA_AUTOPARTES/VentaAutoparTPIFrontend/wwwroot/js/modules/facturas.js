
window.invoicePreviewPayload = null;

window.loadInvoices = async function loadInvoices() {

    moduleContent.innerHTML = `
        <div class="autoparts-panel">
            <p class="text-muted mb-0">
                <span class="spinner-border spinner-border-sm me-2"></span>
                Cargando facturas...
            </p>
        </div>
    `;

    try {

        const data = await apiRequest("/api/Factura");

        if (!Array.isArray(data)) {
            throw new Error("La API no devolvió un listado válido de facturas.");
        }

        invoicesCache = data;

        await renderInvoicesModule();

    } catch (error) {

        console.error("Error al cargar facturas:", error);

        moduleContent.innerHTML = `
            <div class="alert alert-danger">
                <p>${escapeHtml(error.message)}</p>

                <button class="btn btn-outline-danger btn-sm"
                        id="retryInvoicesButton">
                    Reintentar
                </button>
            </div>
        `;

        document
            .getElementById("retryInvoicesButton")
            ?.addEventListener("click", loadInvoices);
    }
}

window.renderInvoicesModule = async function renderInvoicesModule() {

    moduleContent.innerHTML = `
        <div class="autoparts-panel">

            <div class="d-flex justify-content-between align-items-center flex-wrap gap-3 mb-4">

                <div>
                    <h5 class="mb-1">Listado de facturas</h5>
                    <p class="text-muted small mb-0">
                        Consultá, registrá y gestioná las facturas de venta.
                    </p>
                </div>

                <button class="btn btn-primary"
                        id="createInvoiceButton">
                    <i class="bi bi-plus-lg me-2"></i>Nueva factura
                </button>

            </div>

            <div class="row g-3 mb-4">

                <div class="col-lg-4">
                    <label class="form-label" for="invoiceClientFilter">
                        Cliente
                    </label>

                    <select class="form-select" id="invoiceClientFilter">
                        <option value="">Todos los clientes</option>
                    </select>
                </div>

                <div class="col-lg-3">
                    <label class="form-label" for="invoiceStartDate">
                        Fecha desde
                    </label>

                    <input type="date"
                           class="form-control"
                           id="invoiceStartDate">
                </div>

                <div class="col-lg-3">
                    <label class="form-label" for="invoiceEndDate">
                        Fecha hasta
                    </label>

                    <input type="date"
                           class="form-control"
                           id="invoiceEndDate">
                </div>

                <div class="col-lg-2 d-flex align-items-end">
                    <button class="btn btn-outline-primary w-100"
                            id="filterInvoicesButton">
                        <i class="bi bi-search me-1"></i>Filtrar
                    </button>
                </div>

            </div>

            <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-3">

                <span class="text-muted small" id="invoiceCount">
                    ${invoicesCache.length} facturas encontradas
                </span>

                <button class="btn btn-outline-secondary btn-sm"
                        id="refreshInvoicesButton">
                    <i class="bi bi-arrow-clockwise me-1"></i>Actualizar listado
                </button>

            </div>

            <div class="table-responsive">

                <table class="table dashboard-table align-middle">

                    <thead>
                        <tr>
                            <th>Número</th>
                            <th>Fecha</th>
                            <th>Cliente</th>
                            <th>Estado</th>
                            <th>Total</th>
                            <th>Acciones</th>
                        </tr>
                    </thead>

                    <tbody id="invoicesTableBody"></tbody>

                </table>

            </div>

        </div>
    `;

    await loadInvoiceClientFilter();

    document
        .getElementById("createInvoiceButton")
        .addEventListener("click", renderCreateInvoiceForm);

    document
        .getElementById("filterInvoicesButton")
        .addEventListener("click", filterInvoices);

    document
        .getElementById("refreshInvoicesButton")
        .addEventListener("click", loadInvoices);

    renderInvoicesTable(invoicesCache);
}

window.loadInvoiceClientFilter = async function loadInvoiceClientFilter() {

    const select = document.getElementById("invoiceClientFilter");

    try {

        const clients = await apiRequest("/api/Cliente/activos");

        if (!Array.isArray(clients)) {
            throw new Error("La API no devolvió un listado válido de clientes.");
        }

        window.invoiceClientsCache = clients;

        if (select) {
            select.innerHTML = `
                <option value="">Todos los clientes</option>
                ${window.invoiceClientsCache.map(client => `
                    <option value="${escapeHtml(String(client.clienteId))}">
                        ${escapeHtml(getClientDisplayName(client))}
                    </option>
                `).join("")}
            `;
        }

        console.log("Clientes cargados para facturas:", window.invoiceClientsCache);
        return true;

    } catch (error) {

        console.error("Error al cargar clientes para el filtro:", error);
        window.invoiceClientsCache = [];
        return false;
    }
}

window.getClientDisplayName = function getClientDisplayName(client) {

    if (client.razonSocial?.trim()) {
        return client.razonSocial.trim();
    }

    const fullName =
        `${client.nombre ?? ""} ${client.apellido ?? ""}`.trim();

    return fullName || `Cliente ${client.clienteId}`;
}

window.filterInvoices = async function filterInvoices() {

    const clientId =
        document.getElementById("invoiceClientFilter").value;

    const initialDate =
        document.getElementById("invoiceStartDate").value;

    const finalDate =
        document.getElementById("invoiceEndDate").value;

    if ((initialDate && !finalDate) || (!initialDate && finalDate)) {

        alert("Para filtrar por período, ingresá ambas fechas.");

        return;
    }

    if (initialDate && finalDate && initialDate > finalDate) {

        alert("La fecha inicial no puede ser posterior a la fecha final.");

        return;
    }

    const button = document.getElementById("filterInvoicesButton");

    button.disabled = true;
    button.textContent = "Consultando...";

    try {

        let data;

        if (initialDate && finalDate) {

            const query = new URLSearchParams({
                startDate: initialDate,
                endDate: finalDate
            });

            data = await apiRequest(
                `/api/Factura/fecha?${query.toString()}`
            );

        } else if (clientId) {

            data = await apiRequest(
                `/api/Factura/cliente/${encodeURIComponent(clientId)}`
            );

        } else {

            data = await apiRequest("/api/Factura");
        }

        if (!Array.isArray(data)) {
            throw new Error("La API devolvió un resultado no válido.");
        }

        if (clientId && initialDate && finalDate) {

            data = data.filter(
                invoice => String(invoice.clienteId) === clientId
            );
        }

        invoicesCache = data;

        renderInvoicesTable(invoicesCache);

    } catch (error) {

        console.error("Error al filtrar facturas:", error);

        alert(error.message);

    } finally {

        button.disabled = false;

        button.innerHTML =
            '<i class="bi bi-search me-1"></i>Filtrar';
    }
}

window.renderInvoicesTable = function renderInvoicesTable(items) {

    const tableBody = document.getElementById("invoicesTableBody");
    const count = document.getElementById("invoiceCount");

    if (!tableBody || !count) {
        return;
    }

    count.textContent = `${items.length} facturas encontradas`;

    tableBody.innerHTML = "";

    if (items.length === 0) {

        tableBody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-muted py-4">
                    No se encontraron facturas.
                </td>
            </tr>
        `;

        return;
    }

    items.forEach(invoice => {

        const client = window.invoiceClientsCache.find(
            item => String(item.clienteId) === String(invoice.clienteId)
        );

        if (!client) {
            console.warn("No se encontró el cliente de la factura:", {
                facturaId: invoice.facturaId,
                clienteIdFactura: invoice.clienteId,
                clientesCargados: window.invoiceClientsCache
            });
        }

        const clientName = client
            ? getClientDisplayName(client)
            : `Cliente ${invoice.clienteId}`;

        const isCancelled =
            String(invoice.estado).toLowerCase().includes("anul");

        const row = document.createElement("tr");

        row.innerHTML = `
            <td>
                <strong>${escapeHtml(invoice.numeroFactura)}</strong>
            </td>

            <td>${formatDate(invoice.fechaFactura)}</td>

            <td>${escapeHtml(clientName)}</td>

            <td>
                ${isCancelled
                ? '<span class="badge bg-secondary">Anulada</span>'
                : `<span class="badge bg-success">${escapeHtml(invoice.estado)}</span>`}
            </td>

            <td>
                <strong>
                    ${currencyFormatter.format(invoice.total ?? 0)}
                </strong>
            </td>

            <td>
                <div class="d-flex flex-wrap gap-1">

                    <button class="btn btn-sm btn-outline-primary"
                            data-invoice-action="detail"
                            data-id="${invoice.facturaId}"
                            title="Consultar detalle">
                        <i class="bi bi-eye"></i>
                    </button>

                    ${!isCancelled ? `
                        <button class="btn btn-sm btn-outline-danger"
                                data-invoice-action="cancel"
                                data-id="${invoice.facturaId}"
                                title="Anular factura">
                            <i class="bi bi-x-circle"></i>
                        </button>
                    ` : ""}

                </div>
            </td>
        `;

        tableBody.appendChild(row);
    });

    tableBody
        .querySelectorAll("button[data-invoice-action]")
        .forEach(button => {

            button.addEventListener("click", async () => {

                const id = Number(button.dataset.id);
                const action = button.dataset.invoiceAction;

                if (action === "detail") {
                    await renderInvoiceDetail(id);
                }

                if (action === "cancel") {
                    await cancelInvoice(id);
                }
            });
        });
}

window.renderInvoiceDetail = async function renderInvoiceDetail(id) {

    moduleContent.innerHTML = `
        <div class="autoparts-panel">
            <p class="text-muted">
                <span class="spinner-border spinner-border-sm me-2"></span>
                Consultando detalle de factura...
            </p>
        </div>
    `;

    try {

        const invoice = await apiRequest(`/api/Factura/${id}`);

        renderInvoiceDetailView(invoice);

    } catch (error) {

        moduleContent.innerHTML = `
            <div class="alert alert-danger">
                <p>${escapeHtml(error.message)}</p>

                <button class="btn btn-outline-secondary btn-sm"
                        id="backToInvoicesButton">
                    Volver al listado
                </button>
            </div>
        `;

        document
            .getElementById("backToInvoicesButton")
            .addEventListener("click", renderInvoicesModule);
    }
}

window.renderInvoiceDetailView = function renderInvoiceDetailView(invoice) {

    const client = window.invoiceClientsCache.find(
        item => String(item.clienteId) === String(invoice.clienteId)
    );

    const clientName = client
        ? getClientDisplayName(client)
        : `Cliente ${invoice.clienteId}`;

    const details = Array.isArray(invoice.details)
        ? invoice.details
        : [];

    const isCancelled =
        String(invoice.estado).toLowerCase().includes("anul");

    moduleContent.innerHTML = `
        <div class="autoparts-panel">

            <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-4">

                <div>
                    <h5 class="mb-1">
                        Factura ${escapeHtml(invoice.numeroFactura)}
                    </h5>

                    <p class="text-muted small mb-0">
                        Detalle de la operación de venta.
                    </p>
                </div>

                <button class="btn btn-outline-secondary btn-sm"
                        id="backToInvoicesButton">
                    <i class="bi bi-arrow-left me-2"></i>Volver al listado
                </button>

            </div>

            <div class="row g-3 mb-4">

                <div class="col-md-4">
                    <div class="border rounded p-3 h-100">
                        <small class="text-muted">Número de factura</small>
                        <div class="fw-semibold">
                            ${escapeHtml(invoice.numeroFactura)}
                        </div>
                    </div>
                </div>

                <div class="col-md-4">
                    <div class="border rounded p-3 h-100">
                        <small class="text-muted">Fecha</small>
                        <div class="fw-semibold">
                            ${formatDate(invoice.fechaFactura)}
                        </div>
                    </div>
                </div>

                <div class="col-md-4">
                    <div class="border rounded p-3 h-100">
                        <small class="text-muted">Estado</small>
                        <div class="fw-semibold">
                            ${isCancelled
            ? '<span class="badge bg-secondary">Anulada</span>'
            : `<span class="badge bg-success">${escapeHtml(invoice.estado)}</span>`}
                        </div>
                    </div>
                </div>

                <div class="col-md-6">
                    <div class="border rounded p-3 h-100">
                        <small class="text-muted">Cliente</small>
                        <div class="fw-semibold">
                            ${escapeHtml(clientName)}
                        </div>
                    </div>
                </div>

                <div class="col-md-6">
                    <div class="border rounded p-3 h-100">
                        <small class="text-muted">Observaciones</small>
                        <div>
                            ${escapeHtml(invoice.observaciones || "Sin observaciones")}
                        </div>
                    </div>
                </div>

            </div>

            <h6 class="mb-3">Productos facturados</h6>

            <div class="table-responsive">

                <table class="table dashboard-table align-middle">

                    <thead>
                        <tr>
                            <th>Código</th>
                            <th>Autoparte</th>
                            <th>Cantidad</th>
                            <th>Precio unitario</th>
                            <th>Descuento</th>
                            <th>Subtotal</th>
                        </tr>
                    </thead>

                    <tbody>

                        ${details.length === 0 ? `
                            <tr>
                                <td colspan="6"
                                    class="text-center text-muted py-4">
                                    No hay detalles disponibles.
                                </td>
                            </tr>
                        ` : details.map(detail => `
                            <tr>
                                <td>${escapeHtml(detail.codigo)}</td>
                                <td>${escapeHtml(detail.nombre)}</td>
                                <td>${detail.cantidad}</td>
                                <td>
                                    ${currencyFormatter.format(detail.precioUnitario ?? 0)}
                                </td>
                                <td>
                                    ${Number(detail.porcentajeDescuento ?? 0).toFixed(2)} %
                                </td>
                                <td>
                                    ${currencyFormatter.format(detail.subtotal ?? 0)}
                                </td>
                            </tr>
                        `).join("")}

                    </tbody>

                </table>

            </div>

            <div class="d-flex justify-content-end mt-3">

                <div class="border rounded p-3 text-end"
                     style="min-width: 230px;">

                    <small class="text-muted">Total de la factura</small>

                    <h4 class="mb-0 mt-1">
                        ${currencyFormatter.format(invoice.total ?? 0)}
                    </h4>

                </div>

            </div>

            ${!isCancelled ? `
                <div class="d-flex justify-content-end mt-4">
                    <button class="btn btn-outline-danger"
                            id="cancelInvoiceDetailButton">
                        <i class="bi bi-x-circle me-2"></i>Anular factura
                    </button>
                </div>
            ` : ""}

        </div>
    `;

    document
        .getElementById("backToInvoicesButton")
        .addEventListener("click", renderInvoicesModule);

    document
        .getElementById("cancelInvoiceDetailButton")
        ?.addEventListener("click", () => cancelInvoice(invoice.facturaId));
}

window.renderCreateInvoiceForm = async function renderCreateInvoiceForm() {

    moduleContent.innerHTML = `
        <div class="autoparts-panel">
            <p class="text-muted">
                <span class="spinner-border spinner-border-sm me-2"></span>
                Preparando formulario de factura...
            </p>
        </div>
    `;

    try {

        const [clients, autoparts] = await Promise.all([
            apiRequest("/api/Cliente/activos"),
            apiRequest("/api/Autoparte")
        ]);

        if (!Array.isArray(clients) || !Array.isArray(autoparts)) {
            throw new Error("No se pudieron obtener los datos necesarios.");
        }

        invoiceClientsCache = clients;

        invoiceAutopartsCache = autoparts.filter(
            item => item.estado === true
        );

        renderInvoiceForm();

    } catch (error) {

        console.error("Error al preparar la factura:", error);

        moduleContent.innerHTML = `
            <div class="alert alert-danger">
                <p>${escapeHtml(error.message)}</p>

                <button class="btn btn-outline-secondary btn-sm"
                        id="backToInvoicesButton">
                    Volver al listado
                </button>
            </div>
        `;

        document
            .getElementById("backToInvoicesButton")
            .addEventListener("click", renderInvoicesModule);
    }
}

window.renderInvoiceForm = function renderInvoiceForm() {

    invoicePreviewPayload = null;

    moduleContent.innerHTML = `
        <div class="autoparts-panel">

            <div class="mb-4">
                <h5>Registrar factura</h5>
                <p class="text-muted small mb-0">
                    Seleccioná el cliente y agregá los productos de la venta.
                </p>
            </div>

            <form id="invoiceForm">

                <div class="row g-3 mb-4">

                    <div class="col-md-6">

                        <label class="form-label" for="invoiceClient">
                            Cliente *
                        </label>

                        <select class="form-select"
                                id="invoiceClient"
                                required>

                            <option value="">Seleccionar cliente</option>

                            ${invoiceClientsCache.map(client => `
                                <option value="${client.clienteId}">
                                    ${escapeHtml(getClientDisplayName(client))}
                                </option>
                            `).join("")}

                        </select>

                    </div>

                    <div class="col-md-6">

                        <label class="form-label" for="invoiceObservations">
                            Observaciones
                        </label>

                        <input type="text"
                               class="form-control"
                               id="invoiceObservations"
                               maxlength="500"
                               placeholder="Observaciones opcionales">

                    </div>

                </div>

                <hr>

                <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-3">

                    <h6 class="mb-0">Productos de la factura</h6>

                    <button type="button"
                            class="btn btn-outline-primary btn-sm"
                            id="addInvoiceDetailButton">
                        <i class="bi bi-plus-lg me-1"></i>Agregar producto
                    </button>

                </div>

                <div id="invoiceDetailsContainer"></div>

                <div class="alert alert-light border mt-3">
                    <small class="text-muted">
                        Calculá la vista previa para revisar precios, descuentos y total
                        antes de emitir la boleta.
                    </small>
                </div>

                <div id="invoicePreviewContainer" class="mt-4"></div>

                <div class="autoparts-message mt-3"
                     id="invoiceFormMessage"></div>

                <div class="autoparts-actions mt-4">

                    <button type="button"
                            class="btn btn-outline-secondary"
                            id="cancelInvoiceForm">
                        Cancelar
                    </button>

                    <button type="button"
                            class="btn btn-outline-primary"
                            id="previewInvoiceButton">
                        <i class="bi bi-calculator me-2"></i>Calcular total
                    </button>

                    <button type="submit"
                            class="btn btn-primary"
                            id="saveInvoiceButton"
                            disabled>
                        <i class="bi bi-check-lg me-2"></i>Emitir boleta
                    </button>

                </div>

            </form>

        </div>
    `;

    document
        .getElementById("addInvoiceDetailButton")
        .addEventListener("click", addInvoiceDetailRow);

    document
        .getElementById("cancelInvoiceForm")
        .addEventListener("click", renderInvoicesModule);

    document
        .getElementById("previewInvoiceButton")
        .addEventListener("click", previewInvoice);

    document
        .getElementById("invoiceForm")
        .addEventListener("input", invalidateInvoicePreview);

    document
        .getElementById("invoiceForm")
        .addEventListener("change", invalidateInvoicePreview);

    document
        .getElementById("invoiceForm")
        .addEventListener("submit", async event => {

            event.preventDefault();

            await saveInvoice();
        });

    addInvoiceDetailRow();
}

window.addInvoiceDetailRow = function addInvoiceDetailRow() {

    const container =
        document.getElementById("invoiceDetailsContainer");

    const row = document.createElement("div");

    row.className = "invoice-detail-row border rounded p-3 mb-3";

    row.innerHTML = `
        <div class="row g-3 align-items-end">

            <div class="col-lg-6">

                <label class="form-label">
                    Autoparte *
                </label>

                <select class="form-select invoice-autopart-select"
                        required>

                    <option value="">Seleccionar autoparte</option>

                    ${invoiceAutopartsCache.map(item => `
                        <option value="${item.autoparteId}">
                            ${escapeHtml(item.codigo)} -
                            ${escapeHtml(item.nombre)}
                            (Stock: ${item.stockActual ?? 0})
                        </option>
                    `).join("")}

                </select>

                <small class="text-muted invoice-stock-info">
                    Seleccioná una autoparte.
                </small>

            </div>

            <div class="col-lg-3">

                <label class="form-label">
                    Cantidad *
                </label>

                <input type="number"
                       class="form-control invoice-quantity"
                       min="1"
                       step="1"
                       value="1"
                       required>

            </div>

            <div class="col-lg-3">

                <button type="button"
                        class="btn btn-outline-danger w-100 remove-invoice-detail">
                    <i class="bi bi-trash me-1"></i>Quitar
                </button>

            </div>

        </div>

        <div class="small text-muted mt-2 invoice-price-info">
            Precio de referencia: no seleccionado.
        </div>
    `;

    container.appendChild(row);
    invalidateInvoicePreview();

    const select = row.querySelector(".invoice-autopart-select");

    select.addEventListener("change", () => {

        updateInvoiceDetailInformation(row);

        updateInvoiceAutopartOptions();
    });

    row.querySelector(".invoice-quantity")
        .addEventListener("input", () => {
            validateInvoiceDetailRow(row);
        });

    row.querySelector(".remove-invoice-detail")
        .addEventListener("click", () => {

            const rows = container.querySelectorAll(".invoice-detail-row");

            if (rows.length <= 1) {
                alert("La factura debe contener al menos un producto.");
                return;
            }

            row.remove();
            invalidateInvoicePreview();

            updateInvoiceAutopartOptions();
        });

    updateInvoiceAutopartOptions();
}

window.updateInvoiceDetailInformation = function updateInvoiceDetailInformation(row) {

    const select = row.querySelector(".invoice-autopart-select");
    const stockInfo = row.querySelector(".invoice-stock-info");
    const priceInfo = row.querySelector(".invoice-price-info");

    const autopart = invoiceAutopartsCache.find(
        item => String(item.autoparteId) === select.value
    );

    if (!autopart) {

        stockInfo.textContent = "Seleccioná una autoparte.";

        priceInfo.textContent =
            "Precio de referencia: no seleccionado.";

        return;
    }

    stockInfo.textContent =
        `Stock disponible: ${autopart.stockActual} unidades.`;

    priceInfo.textContent =
        `Precio actual de referencia: ${currencyFormatter.format(autopart.precioActual ?? 0)}.`;

    validateInvoiceDetailRow(row);
}

window.validateInvoiceDetailRow = function validateInvoiceDetailRow(row) {

    const select = row.querySelector(".invoice-autopart-select");
    const quantityInput = row.querySelector(".invoice-quantity");
    const autopart = invoiceAutopartsCache.find(
        item => String(item.autoparteId) === select.value
    );

    const quantity = Number(quantityInput.value);

    if (!autopart || !quantityInput.value) {
        quantityInput.setCustomValidity("");
        return true;
    }

    if (!Number.isInteger(quantity) || quantity <= 0) {

        quantityInput.setCustomValidity(
            "La cantidad debe ser un número entero mayor que cero."
        );

        return false;
    }

    if (quantity > autopart.stockActual) {

        quantityInput.setCustomValidity(
            `La cantidad supera el stock disponible (${autopart.stockActual}).`
        );

        return false;
    }

    quantityInput.setCustomValidity("");

    return true;
}

window.updateInvoiceAutopartOptions = function updateInvoiceAutopartOptions() {

    const rows = document.querySelectorAll(".invoice-detail-row");

    const selectedIds = Array.from(rows)
        .map(row => row.querySelector(".invoice-autopart-select").value)
        .filter(Boolean);

    rows.forEach(row => {

        const select = row.querySelector(".invoice-autopart-select");
        const currentValue = select.value;

        Array.from(select.options).forEach(option => {

            if (!option.value) {
                return;
            }

            option.disabled =
                selectedIds.includes(option.value) &&
                option.value !== currentValue;
        });
    });
}

window.invalidateInvoicePreview = function invalidateInvoicePreview() {

    invoicePreviewPayload = null;

    const saveButton = document.getElementById("saveInvoiceButton");
    const previewContainer = document.getElementById("invoicePreviewContainer");

    if (saveButton) {
        saveButton.disabled = true;
    }

    if (previewContainer) {
        previewContainer.innerHTML = `
            <div class="alert alert-warning mb-0">
                Los datos cambiaron. Volvé a calcular el total para emitir la boleta.
            </div>
        `;
    }
}

window.previewInvoice = async function previewInvoice() {

    const message = document.getElementById("invoiceFormMessage");
    const previewButton = document.getElementById("previewInvoiceButton");
    const saveButton = document.getElementById("saveInvoiceButton");
    const previewContainer = document.getElementById("invoicePreviewContainer");

    const clientId = Number(document.getElementById("invoiceClient").value);
    const observations = document.getElementById("invoiceObservations").value.trim();
    const rows = Array.from(document.querySelectorAll(".invoice-detail-row"));

    if (!clientId) {
        showAutopartMessage(message, "Seleccioná un cliente para continuar.", "error");
        return;
    }

    if (rows.length === 0) {
        showAutopartMessage(message, "Agregá al menos un producto a la boleta.", "error");
        return;
    }

    const details = [];

    for (const row of rows) {
        const autopartId = Number(row.querySelector(".invoice-autopart-select").value);
        const quantityInput = row.querySelector(".invoice-quantity");
        const quantity = Number(quantityInput.value);

        if (!autopartId) {
            showAutopartMessage(message, "Seleccioná una autoparte en todas las filas.", "error");
            return;
        }

        if (!Number.isInteger(quantity) || quantity <= 0) {
            showAutopartMessage(message, "Todas las cantidades deben ser números enteros mayores que cero.", "error");
            return;
        }

        const autopart = invoiceAutopartsCache.find(item => String(item.autoparteId) === String(autopartId));

        if (!autopart || autopart.estado !== true) {
            showAutopartMessage(message, "Una de las autopartes seleccionadas no está disponible.", "error");
            return;
        }

        if (quantity > autopart.stockActual) {
            showAutopartMessage(message, `Stock insuficiente para ${autopart.nombre}. Disponible: ${autopart.stockActual}.`, "error");
            return;
        }

        details.push({ autoparteId: autopartId, cantidad: quantity });
    }

    if (new Set(details.map(item => item.autoparteId)).size !== details.length) {
        showAutopartMessage(message, "No se puede repetir una autoparte en la misma boleta.", "error");
        return;
    }

    const payload = {
        clienteId: clientId,
        observaciones: observations || null,
        details
    };

    previewButton.disabled = true;
    previewButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Calculando...';
    saveButton.disabled = true;
    previewContainer.innerHTML = "";

    try {
        const preview = await apiRequest("/api/Factura/preview", {
            method: "POST",
            body: JSON.stringify(payload)
        });

        invoicePreviewPayload = JSON.stringify(payload);

        previewContainer.innerHTML = `
            <div class="border rounded p-3">
                <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-3">
                    <h6 class="mb-0">Vista previa de la boleta</h6>
                    <span class="badge bg-success">Total calculado</span>
                </div>
                <div class="table-responsive">
                    <table class="table dashboard-table align-middle mb-0">
                        <thead>
                            <tr>
                                <th>Código</th>
                                <th>Autoparte</th>
                                <th>Cantidad</th>
                                <th>Precio unitario</th>
                                <th>Descuento</th>
                                <th>Subtotal</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${(preview.details || []).map(item => `
                                <tr>
                                    <td>${escapeHtml(item.codigo)}</td>
                                    <td>${escapeHtml(item.nombre)}</td>
                                    <td>${item.cantidad}</td>
                                    <td>${currencyFormatter.format(item.precioUnitario ?? 0)}</td>
                                    <td>${Number(item.porcentajeDescuento ?? 0).toFixed(2)} %</td>
                                    <td>${currencyFormatter.format(item.subtotal ?? 0)}</td>
                                </tr>
                            `).join("")}
                        </tbody>
                    </table>
                </div>
                <div class="d-flex justify-content-end mt-3">
                    <div class="text-end">
                        <small class="text-muted">Total a emitir</small>
                        <h4 class="mb-0">${currencyFormatter.format(preview.total ?? 0)}</h4>
                    </div>
                </div>
                <p class="text-muted small mb-0 mt-3">
                    Esta es una vista previa. Todavía no se registró la factura ni se modificó el stock.
                </p>
            </div>
        `;

        saveButton.disabled = false;
        showAutopartMessage(message, "Vista previa calculada. Revisá los datos y emití la boleta cuando estés conforme.", "success");

    } catch (error) {
        invoicePreviewPayload = null;
        previewContainer.innerHTML = "";
        showAutopartMessage(message, error.message, "error");
    } finally {
        previewButton.disabled = false;
        previewButton.innerHTML = '<i class="bi bi-calculator me-2"></i>Calcular total';
    }
}

window.saveInvoice = async function saveInvoice() {

    const message = document.getElementById("invoiceFormMessage");
    const button = document.getElementById("saveInvoiceButton");

    const clientId = Number(
        document.getElementById("invoiceClient").value
    );

    const observations = document
        .getElementById("invoiceObservations")
        .value.trim();

    const rows = Array.from(
        document.querySelectorAll(".invoice-detail-row")
    );

    if (!clientId) {

        showAutopartMessage(
            message,
            "Seleccioná un cliente para continuar.",
            "error"
        );

        return;
    }

    if (rows.length === 0) {

        showAutopartMessage(
            message,
            "Agregá al menos un producto a la factura.",
            "error"
        );

        return;
    }

    const details = [];

    for (const row of rows) {

        const autopartId = Number(
            row.querySelector(".invoice-autopart-select").value
        );

        const quantity = Number(
            row.querySelector(".invoice-quantity").value
        );

        if (!autopartId) {

            showAutopartMessage(
                message,
                "Seleccioná una autoparte en todas las filas.",
                "error"
            );

            return;
        }

        if (!Number.isInteger(quantity) || quantity <= 0) {

            showAutopartMessage(
                message,
                "Todas las cantidades deben ser números enteros mayores que cero.",
                "error"
            );

            return;
        }

        const autopart = invoiceAutopartsCache.find(
            item => String(item.autoparteId) === String(autopartId)
        );

        if (!autopart || autopart.estado !== true) {

            showAutopartMessage(
                message,
                "Una de las autopartes seleccionadas no está disponible.",
                "error"
            );

            return;
        }

        if (quantity > autopart.stockActual) {

            showAutopartMessage(
                message,
                `Stock insuficiente para ${autopart.nombre}. Disponible: ${autopart.stockActual}.`,
                "error"
            );

            return;
        }

        details.push({
            autoparteId: autopartId,
            cantidad: quantity
        });
    }

    const uniqueIds = new Set(
        details.map(detail => detail.autoparteId)
    );

    if (uniqueIds.size !== details.length) {

        showAutopartMessage(
            message,
            "No se puede repetir una autoparte en la misma factura.",
            "error"
        );

        return;
    }

    const payload = {
        clienteId: clientId,
        observaciones: observations || null,
        details: details
    };

    if (!invoicePreviewPayload || invoicePreviewPayload !== JSON.stringify(payload)) {
        showAutopartMessage(
            message,
            "Primero calculá la vista previa de la boleta. Si modificaste algún dato, volvé a calcular el total.",
            "error"
        );
        button.disabled = true;
        return;
    }

    button.disabled = true;
    button.innerHTML =
        '<span class="spinner-border spinner-border-sm me-2"></span>Registrando...';

    try {

        const response = await apiRequest(
            "/api/Factura",
            {
                method: "POST",
                body: JSON.stringify(payload)
            }
        );

        await loadInvoices();

        if (response?.numeroFactura) {

            showModuleSuccess(
                `Factura ${response.numeroFactura} registrada correctamente.`
            );

        } else {

            showModuleSuccess(
                "Factura registrada correctamente."
            );
        }

    } catch (error) {

        console.error("Error al registrar factura:", error);

        showAutopartMessage(
            message,
            error.message,
            "error"
        );

        button.disabled = false;

        button.innerHTML =
            '<i class="bi bi-check-lg me-2"></i>Emitir boleta';
    }
}


window.cancelInvoice = async function cancelInvoice(id) {

    const invoice = invoicesCache.find(
        item => item.facturaId === id
    );

    const invoiceNumber = invoice?.numeroFactura || id;

    const confirmed = confirm(
        `¿Estás seguro de que querés anular la factura ${invoiceNumber}?`
    );

    if (!confirmed) {
        return;
    }

    try {

        await apiRequest(
            `/api/Factura/${id}/anular`,
            {
                method: "POST"
            }
        );

        await loadInvoices();

        showModuleSuccess(
            `La factura ${invoiceNumber} fue anulada correctamente.`
        );

    } catch (error) {

        console.error("Error al anular factura:", error);

        alert(error.message);
    }
}
