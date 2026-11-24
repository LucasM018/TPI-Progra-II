
window.loadAutoparts = async function loadAutoparts() {

    moduleContent.innerHTML = `
        <div class="autoparts-panel">
            <p class="text-muted mb-0">
                <span class="spinner-border spinner-border-sm me-2"></span>
                Cargando autopartes...
            </p>
        </div>
    `;

    try {

        const data = await apiRequest("/api/Autoparte");

        if (!Array.isArray(data)) {
            throw new Error("La API no devolvió un listado válido");
        }

        autopartsCache = data;

        renderAutopartsModule();

    } catch (error) {

        console.error("Error al cargar autopartes:", error);

        moduleContent.innerHTML = `
            <div class="alert alert-danger">
                <p class="mb-2">${escapeHtml(error.message)}</p>
                <button class="btn btn-outline-danger btn-sm"
                        id="retryAutopartsButton">
                    Reintentar
                </button>
            </div>
        `;

        document
            .getElementById("retryAutopartsButton")
            ?.addEventListener("click", loadAutoparts);
    }
}

window.renderAutopartsModule = function renderAutopartsModule() {

    moduleContent.innerHTML = `
        <div class="autoparts-panel">

            <div class="d-flex justify-content-between align-items-center flex-wrap gap-3 mb-4">
                <div>
                    <h5 class="mb-1">Listado de autopartes</h5>
                    <p class="text-muted small mb-0">
                        Consultá y administrá las autopartes registradas.
                    </p>
                </div>

                <button class="btn btn-primary" id="createAutopartButton">
                    <i class="bi bi-plus-lg me-2"></i>Nueva autoparte
                </button>
            </div>

            <div class="row g-3 mb-4">

                <div class="col-lg-4">
                    <label for="autopartSearch" class="form-label">
                        Buscar autoparte
                    </label>
                    <input type="text"
                           class="form-control"
                           id="autopartSearch"
                           placeholder="Código o nombre">
                </div>

                <div class="col-lg-4">
                    <label for="autopartCategoryFilter" class="form-label">
                        Categoría
                    </label>
                    <select class="form-select" id="autopartCategoryFilter">
                        <option value="">Todas las categorías</option>
                        ${categories.map(category => `
                            <option value="${category.id}">
                                ${escapeHtml(category.name)}
                            </option>
                        `).join("")}
                    </select>
                </div>

                <div class="col-lg-4">
                    <label for="autopartStatusFilter" class="form-label">
                        Estado
                    </label>
                    <select class="form-select" id="autopartStatusFilter">
                        <option value="">Todos los estados</option>
                        <option value="true">Activas</option>
                        <option value="false">Inactivas</option>
                    </select>
                </div>

            </div>

            <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-3">

                <span class="text-muted small" id="autopartCount">
                    ${autopartsCache.length} autopartes encontradas
                </span>

                <div class="d-flex gap-2">
                    <button class="btn btn-outline-secondary btn-sm"
                            id="clearAutopartFilters">
                        Limpiar filtros
                    </button>

                    <button class="btn btn-outline-primary btn-sm"
                            id="refreshAutopartsButton">
                        <i class="bi bi-arrow-clockwise me-1"></i>Actualizar
                    </button>
                </div>

            </div>

            <div class="table-responsive">
                <table class="table dashboard-table align-middle">
                    <thead>
                        <tr>
                            <th>Código</th>
                            <th>Autoparte</th>
                            <th>Categoría</th>
                            <th>Marca</th>
                            <th>Stock actual</th>
                            <th>Stock mínimo</th>
                            <th>Precio</th>
                            <th>Estado</th>
                            <th>Acciones</th>
                        </tr>
                    </thead>

                    <tbody id="autopartsTableBody"></tbody>
                </table>
            </div>

        </div>
    `;

    document
        .getElementById("autopartSearch")
        .addEventListener("input", applyAutopartsFilters);

    document
        .getElementById("autopartCategoryFilter")
        .addEventListener("change", applyAutopartsFilters);

    document
        .getElementById("autopartStatusFilter")
        .addEventListener("change", applyAutopartsFilters);

    document
        .getElementById("clearAutopartFilters")
        .addEventListener("click", clearAutopartsFilters);

    document
        .getElementById("refreshAutopartsButton")
        .addEventListener("click", loadAutoparts);

    document
        .getElementById("createAutopartButton")
        .addEventListener("click", renderCreateAutopartForm);

    applyAutopartsFilters();
}

window.applyAutopartsFilters = function applyAutopartsFilters() {

    const search = document
        .getElementById("autopartSearch")
        .value
        .trim()
        .toLowerCase();

    const category = document
        .getElementById("autopartCategoryFilter")
        .value;

    const status = document
        .getElementById("autopartStatusFilter")
        .value;

    const filtered = autopartsCache.filter(item => {

        const matchesSearch =
            (item.codigo || "").toLowerCase().includes(search) ||
            (item.nombre || "").toLowerCase().includes(search);

        const matchesCategory =
            !category ||
            String(item.categoriaAutoparteId) === category;

        const matchesStatus =
            status === "" ||
            String(item.estado) === status;

        return matchesSearch && matchesCategory && matchesStatus;
    });

    renderAutopartsTable(filtered);
}
window.renderAutopartsTable = function renderAutopartsTable(items) {

    const tableBody = document.getElementById("autopartsTableBody");
    const count = document.getElementById("autopartCount");

    count.textContent = `${items.length} autopartes encontradas`;

    tableBody.innerHTML = "";

    if (items.length === 0) {

        tableBody.innerHTML = `
            <tr>
                <td colspan="9" class="text-center text-muted py-4">
                    No se encontraron autopartes.
                </td>
            </tr>
        `;

        return;
    }

    items.forEach(item => {

        const category = categories.find(
            c => c.id === item.categoriaAutoparteId
        );

        const row = document.createElement("tr");

        row.innerHTML = `
            <td>${escapeHtml(item.codigo)}</td>

            <td>
                <strong>${escapeHtml(item.nombre)}</strong>
                <div class="small text-muted">
                    ${escapeHtml(item.descripcion || "")}
                </div>
            </td>

            <td>
                ${escapeHtml(category?.name || "Sin categoría")}
                <div class="small text-muted">
                    ${escapeHtml(category?.description || "")}
                </div>
            </td>

            <td>${escapeHtml(item.marca || "-")}</td>

            <td>${item.stockActual ?? 0}</td>

            <td>${item.stockMinimo ?? 0}</td>

            <td>${currencyFormatter.format(item.precioActual ?? 0)}</td>

            <td>
                ${item.estado
                ? '<span class="badge bg-success">Activa</span>'
                : '<span class="badge bg-secondary">Inactiva</span>'}
            </td>

            <td>
                <div class="d-flex flex-wrap gap-1">

                    <button class="btn btn-sm btn-outline-primary"
                            data-action="edit"
                            data-id="${item.autoparteId}"
                            title="Editar">
                        <i class="bi bi-pencil"></i>
                    </button>

                    <button class="btn btn-sm btn-outline-success"
                            data-action="stock-in"
                            data-id="${item.autoparteId}"
                            title="Entrada de stock"
                            ${!item.estado ? "disabled" : ""}>
                        <i class="bi bi-box-arrow-in-down"></i>
                    </button>

                    <button class="btn btn-sm btn-outline-warning"
                            data-action="stock-out"
                            data-id="${item.autoparteId}"
                            title="Salida de stock"
                            ${!item.estado ? "disabled" : ""}>
                        <i class="bi bi-box-arrow-up"></i>
                    </button>

                    <button class="btn btn-sm btn-outline-info"
                            data-action="price"
                            data-id="${item.autoparteId}"
                            title="Actualizar precio"
                            ${!item.estado ? "disabled" : ""}>
                        <i class="bi bi-currency-dollar"></i>
                    </button>

                    <button class="btn btn-sm btn-outline-dark"
                            data-action="history"
                            data-id="${item.autoparteId}"
                            title="Historial de precios">
                        <i class="bi bi-clock-history"></i>
                    </button>

                    <button class="btn btn-sm btn-outline-secondary"
                            data-action="stock-history"
                            data-id="${item.autoparteId}"
                            title="Historial de stock">
                        <i class="bi bi-box-seam"></i>
                    </button>

                    <button class="btn btn-sm ${item.estado
                ? "btn-outline-danger"
                : "btn-outline-success"}"
                            data-action="status"
                            data-id="${item.autoparteId}"
                            title="${item.estado ? "Dar de baja" : "Reactivar"}">
                        <i class="bi ${item.estado
                ? "bi-toggle-on"
                : "bi-toggle-off"}"></i>
                    </button>

                </div>
            </td>
        `;

        tableBody.appendChild(row);
    });

    tableBody.querySelectorAll("button[data-action]")
        .forEach(button => {

            button.addEventListener("click", () => {

                const id = Number(button.dataset.id);
                const action = button.dataset.action;

                handleAutopartAction(action, id);
            });
        });
}

window.clearAutopartsFilters = function clearAutopartsFilters() {

    document.getElementById("autopartSearch").value = "";
    document.getElementById("autopartCategoryFilter").value = "";
    document.getElementById("autopartStatusFilter").value = "";

    applyAutopartsFilters();
}

window.handleAutopartAction = async function handleAutopartAction(action, id) {

    switch (action) {

        case "edit":
            await renderEditAutopartForm(id);
            break;

        case "stock-in":
            renderStockForm(id, "entrada");
            break;

        case "stock-out":
            renderStockForm(id, "salida");
            break;

        case "price":
            renderPriceForm(id);
            break;

        case "history":
            await renderPriceHistory(id);
            break;

        case "stock-history":
            await renderStockHistory(id);
            break;

        case "status":
            await changeAutopartStatus(id);
            break;
    }
}

window.getAutopartById = async function getAutopartById(id) {

    return await apiRequest(`/api/Autoparte/${id}`);
}

window.renderCreateAutopartForm = function renderCreateAutopartForm() {

    renderAutopartForm(null);
}

window.renderEditAutopartForm = async function renderEditAutopartForm(id) {

    try {

        const autopart = await getAutopartById(id);

        renderAutopartForm(autopart);

    } catch (error) {

        alert(error.message);
    }
}

window.renderAutopartForm = function renderAutopartForm(autopart) {

    const isEditing = autopart !== null;

    moduleContent.innerHTML = `
        <div class="autoparts-panel">

            <div class="mb-4">
                <h5>${isEditing ? "Modificar autoparte" : "Registrar autoparte"}</h5>
                <p class="text-muted small mb-0">
                    ${isEditing
            ? "Modificá los datos generales de la autoparte."
            : "Completá los datos para registrar una nueva autoparte."}
                </p>
            </div>

            <form id="autopartForm">

                <div class="row g-3">

                    <div class="col-md-6">
                        <label class="form-label" for="autopartCategory">
                            Categoría *
                        </label>

                        <select class="form-select"
                                id="autopartCategory"
                                required>
                            <option value="">Seleccionar categoría</option>

                            ${categories.map(category => `
                                <option value="${category.id}"
                                    ${isEditing &&
                    autopart.categoriaAutoparteId === category.id
                    ? "selected"
                    : ""}>
                                    ${escapeHtml(category.name)}
                                </option>
                            `).join("")}
                        </select>
                    </div>

                    <div class="col-md-6">
                        <label class="form-label" for="autopartCode">
                            Código *
                        </label>

                        <input type="text"
                               class="form-control"
                               id="autopartCode"
                               maxlength="30"
                               value="${escapeHtml(autopart?.codigo || "")}"
                               required>
                    </div>

                    <div class="col-md-6">
                        <label class="form-label" for="autopartName">
                            Nombre *
                        </label>

                        <input type="text"
                               class="form-control"
                               id="autopartName"
                               maxlength="150"
                               value="${escapeHtml(autopart?.nombre || "")}"
                               required>
                    </div>

                    <div class="col-md-6">
                        <label class="form-label" for="autopartBrand">
                            Marca
                        </label>

                        <input type="text"
                               class="form-control"
                               id="autopartBrand"
                               maxlength="100"
                               value="${escapeHtml(autopart?.marca || "")}">
                    </div>

                    <div class="col-md-12">
                        <label class="form-label" for="autopartDescription">
                            Descripción
                        </label>

                        <textarea class="form-control"
                                  id="autopartDescription"
                                  maxlength="300">${escapeHtml(autopart?.descripcion || "")}</textarea>
                    </div>

                    <div class="col-md-12">
                        <label class="form-label" for="autopartApplication">
                            Aplicación / Modelo
                        </label>

                        <input type="text"
                               class="form-control"
                               id="autopartApplication"
                               maxlength="100"
                               value="${escapeHtml(autopart?.aplicacionModelo || "")}">
                    </div>

                    ${!isEditing ? `
                        <div class="col-md-4">
                            <label class="form-label" for="initialStock">
                                Stock inicial
                            </label>

                            <input type="number"
                                   class="form-control"
                                   id="initialStock"
                                   min="0"
                                   value="0"
                                   required>
                        </div>

                        <div class="col-md-4">
                            <label class="form-label" for="minimumStock">
                                Stock mínimo
                            </label>

                            <input type="number"
                                   class="form-control"
                                   id="minimumStock"
                                   min="0"
                                   value="0"
                                   required>
                        </div>

                        <div class="col-md-4">
                            <label class="form-label" for="initialPrice">
                                Precio inicial
                            </label>

                            <input type="number"
                                   class="form-control"
                                   id="initialPrice"
                                   min="0.01"
                                   step="0.01"
                                   required>
                        </div>
                    ` : ""}

                </div>

                <div class="autoparts-message mt-3"
                     id="autopartFormMessage"></div>

                <div class="autoparts-actions mt-4">

                    <button type="button"
                            class="btn btn-outline-secondary"
                            id="cancelAutopartForm">
                        Cancelar
                    </button>

                    <button type="submit"
                            class="btn btn-primary"
                            id="saveAutopartButton">
                        ${isEditing ? "Guardar cambios" : "Registrar autoparte"}
                    </button>

                </div>

            </form>
        </div>
    `;

    document
        .getElementById("cancelAutopartForm")
        .addEventListener("click", renderAutopartsModule);

    document
        .getElementById("autopartForm")
        .addEventListener("submit", async event => {

            event.preventDefault();

            await saveAutopart(isEditing, autopart?.autoparteId);
        });
}

window.saveAutopart = async function saveAutopart(isEditing, id) {

    const message = document.getElementById("autopartFormMessage");
    const saveButton = document.getElementById("saveAutopartButton");

    const categoryId = Number(
        document.getElementById("autopartCategory").value
    );

    const code = document.getElementById("autopartCode").value.trim();
    const name = document.getElementById("autopartName").value.trim();

    if (!categoryId || !code || !name) {

        showAutopartMessage(
            message,
            "Completá los campos obligatorios.",
            "error"
        );

        return;
    }

    const payload = {
        categoriaAutoparteId: categoryId,
        codigo: code,
        nombre: name,
        descripcion:
            document.getElementById("autopartDescription").value.trim(),
        marca:
            document.getElementById("autopartBrand").value.trim(),
        aplicacionModelo:
            document.getElementById("autopartApplication").value.trim()
    };

    if (!isEditing) {

        payload.stockInicial = Number(
            document.getElementById("initialStock").value
        );

        payload.stockMinimo = Number(
            document.getElementById("minimumStock").value
        );

        payload.precioInicial = Number(
            document.getElementById("initialPrice").value
        );

        if (
            !Number.isInteger(payload.stockInicial) ||
            payload.stockInicial < 0 ||
            !Number.isInteger(payload.stockMinimo) ||
            payload.stockMinimo < 0 ||
            !Number.isFinite(payload.precioInicial) ||
            payload.precioInicial <= 0
        ) {

            showAutopartMessage(
                message,
                "Verificá los valores de stock y precio.",
                "error"
            );

            return;
        }
    }

    saveButton.disabled = true;
    saveButton.textContent = "Guardando...";

    try {

        await apiRequest(
            isEditing
                ? `/api/Autoparte/${id}`
                : "/api/Autoparte",
            {
                method: isEditing ? "PUT" : "POST",
                body: JSON.stringify(payload)
            }
        );

        await loadAutoparts();

        showModuleSuccess(
            isEditing
                ? "Autoparte modificada correctamente."
                : "Autoparte registrada correctamente."
        );

    } catch (error) {

        showAutopartMessage(message, error.message, "error");

        saveButton.disabled = false;

        saveButton.textContent =
            isEditing ? "Guardar cambios" : "Registrar autoparte";
    }
}

window.renderStockForm = function renderStockForm(id, operation) {

    const autopart = autopartsCache.find(
        item => item.autoparteId === id
    );

    if (!autopart) {
        alert("No se encontró la autoparte seleccionada");
        return;
    }

    const isEntry = operation === "entrada";

    moduleContent.innerHTML = `
        <div class="autoparts-panel">

            <h5>${isEntry ? "Entrada de stock" : "Salida de stock"}</h5>

            <p class="text-muted small mb-4">
                ${escapeHtml(autopart.codigo)} -
                ${escapeHtml(autopart.nombre)}
            </p>

            <div class="alert alert-light border">
                Stock actual:
                <strong>${autopart.stockActual}</strong> unidades
            </div>

            <form id="stockForm">

                <div class="mb-3">
                    <label class="form-label" for="stockQuantity">
                        Cantidad *
                    </label>

                    <input type="number"
                           class="form-control"
                           id="stockQuantity"
                           min="1"
                           step="1"
                           required>
                </div>

                <div class="mb-3">
                    <label class="form-label" for="stockObservations">
                        Observaciones *
                    </label>

                    <textarea class="form-control"
                              id="stockObservations"
                              maxlength="300"
                              required></textarea>
                </div>

                <div class="autoparts-message mb-3"
                     id="stockFormMessage"></div>

                <div class="autoparts-actions">

                    <button type="button"
                            class="btn btn-outline-secondary"
                            id="cancelStockForm">
                        Cancelar
                    </button>

                    <button type="submit"
                            class="btn btn-primary"
                            id="saveStockButton">
                        Registrar ${isEntry ? "entrada" : "salida"}
                    </button>

                </div>

            </form>
        </div>
    `;

    document
        .getElementById("cancelStockForm")
        .addEventListener("click", renderAutopartsModule);

    document
        .getElementById("stockForm")
        .addEventListener("submit", async event => {

            event.preventDefault();

            const quantity = Number(
                document.getElementById("stockQuantity").value
            );

            const observations = document
                .getElementById("stockObservations")
                .value.trim();

            const message =
                document.getElementById("stockFormMessage");

            if (
                !Number.isInteger(quantity) ||
                quantity <= 0 ||
                !observations
            ) {

                showAutopartMessage(
                    message,
                    "Ingresá una cantidad válida y las observaciones.",
                    "error"
                );

                return;
            }

            const button =
                document.getElementById("saveStockButton");

            button.disabled = true;
            button.textContent = "Registrando...";

            try {

                await apiRequest(
                    `/api/Autoparte/${id}/stock/${operation}`,
                    {
                        method: "POST",
                        body: JSON.stringify({
                            cantidad: quantity,
                            observaciones: observations
                        })
                    }
                );

                await loadAutoparts();

                showModuleSuccess(
                    isEntry
                        ? "Entrada de stock registrada correctamente"
                        : "Salida de stock registrada correctamente"
                );

            } catch (error) {

                showAutopartMessage(
                    message,
                    error.message,
                    "error"
                );

                button.disabled = false;

                button.textContent =
                    `Registrar ${isEntry ? "entrada" : "salida"}`;
            }
        });
}
window.renderPriceForm = function renderPriceForm(id) {

    const autopart = autopartsCache.find(
        item => item.autoparteId === id
    );

    if (!autopart) {
        alert("No se encontró la autoparte seleccionada.");
        return;
    }

    moduleContent.innerHTML = `
        <div class="autoparts-panel">

            <h5>Actualizar precio</h5>

            <p class="text-muted small mb-4">
                ${escapeHtml(autopart.codigo)} -
                ${escapeHtml(autopart.nombre)}
            </p>

            <div class="alert alert-light border">
                Precio actual:
                <strong>
                    ${currencyFormatter.format(autopart.precioActual ?? 0)}
                </strong>
            </div>

            <form id="priceForm">

                <div class="mb-3">
                    <label class="form-label" for="newPrice">
                        Nuevo precio *
                    </label>

                    <input type="number"
                           class="form-control"
                           id="newPrice"
                           min="0.01"
                           step="0.01"
                           required>
                </div>

                <div class="autoparts-message mb-3"
                     id="priceFormMessage"></div>

                <div class="autoparts-actions">

                    <button type="button"
                            class="btn btn-outline-secondary"
                            id="cancelPriceForm">
                        Cancelar
                    </button>

                    <button type="submit"
                            class="btn btn-primary"
                            id="savePriceButton">
                        Actualizar precio
                    </button>

                </div>

            </form>
        </div>
    `;

    document
        .getElementById("cancelPriceForm")
        .addEventListener("click", renderAutopartsModule);

    document
        .getElementById("priceForm")
        .addEventListener("submit", async event => {

            event.preventDefault();

            const price = Number(
                document.getElementById("newPrice").value
            );

            const message =
                document.getElementById("priceFormMessage");

            if (!Number.isFinite(price) || price <= 0) {

                showAutopartMessage(
                    message,
                    "Ingresá un precio mayor que cero.",
                    "error"
                );

                return;
            }

            const button =
                document.getElementById("savePriceButton");

            button.disabled = true;
            button.textContent = "Actualizando...";

            try {

                await apiRequest(
                    `/api/Autoparte/${id}/precios`,
                    {
                        method: "POST",
                        body: JSON.stringify({
                            precio: price
                        })
                    }
                );

                await loadAutoparts();

                showModuleSuccess(
                    "Precio actualizado correctamente."
                );

            } catch (error) {

                showAutopartMessage(
                    message,
                    error.message,
                    "error"
                );

                button.disabled = false;
                button.textContent = "Actualizar precio";
            }
        });
}

window.renderPriceHistory = async function renderPriceHistory(id) {

    const autopart = autopartsCache.find(
        item => item.autoparteId === id
    );

    moduleContent.innerHTML = `
        <div class="autoparts-panel">
            <p class="text-muted mb-0">
                <span class="spinner-border spinner-border-sm me-2"></span>
                Consultando historial de precios...
            </p>
        </div>
    `;

    try {

        const history = await apiRequest(
            `/api/Autoparte/${id}/precios`
        );

        const records = Array.isArray(history) ? history : [];

        moduleContent.innerHTML = `
            <div class="autoparts-panel">

                <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-4">

                    <div>
                        <h5 class="mb-1">Historial de precios</h5>

                        <p class="text-muted small mb-0">
                            ${escapeHtml(autopart?.codigo || "")} -
                            ${escapeHtml(autopart?.nombre || "")}
                        </p>
                    </div>

                    <button class="btn btn-outline-secondary btn-sm"
                            id="backFromPriceHistory">
                        <i class="bi bi-arrow-left me-2"></i>Volver
                    </button>

                </div>

                <div class="table-responsive">

                    <table class="table dashboard-table align-middle">

                        <thead>
                            <tr>
                                <th>Precio</th>
                                <th>Fecha desde</th>
                                <th>Fecha hasta</th>
                            </tr>
                        </thead>

                        <tbody id="priceHistoryTableBody">

                            ${records.length === 0 ? `
                                <tr>
                                    <td colspan="3"
                                        class="text-center text-muted py-4">
                                        No hay registros en el historial.
                                    </td>
                                </tr>
                            ` : records.map(record => `
                                <tr>
                                    <td>
                                        ${currencyFormatter.format(
            Number(record.precio ?? 0)
        )}
                                    </td>
                                    <td>
                                        ${formatDate(record.fechaDesde)}
                                    </td>
                                    <td>
                                        ${record.fechaHasta
                ? formatDate(record.fechaHasta)
                : "Vigente"}
                                    </td>
                                </tr>
                            `).join("")}

                        </tbody>

                    </table>

                </div>
            </div>
        `;

        document
            .getElementById("backFromPriceHistory")
            .addEventListener("click", renderAutopartsModule);

    } catch (error) {

        moduleContent.innerHTML = `
            <div class="alert alert-danger">
                <p>${escapeHtml(error.message)}</p>

                <button class="btn btn-outline-secondary btn-sm"
                        id="backFromPriceHistory">
                    Volver
                </button>
            </div>
        `;

        document
            .getElementById("backFromPriceHistory")
            .addEventListener("click", renderAutopartsModule);
    }
}

window.renderStockHistory = async function renderStockHistory(id) {

    const autopart = autopartsCache.find(
        item => item.autoparteId === id
    );

    moduleContent.innerHTML = `
        <div class="autoparts-panel">
            <p class="text-muted mb-0">
                <span class="spinner-border spinner-border-sm me-2"></span>
                Consultando historial de stock...
            </p>
        </div>
    `;

    try {

        const history = await apiRequest(
            `/api/MovimientoStock/autoparte/${id}`
        );

        const records = Array.isArray(history) ? history : [];

        moduleContent.innerHTML = `
            <div class="autoparts-panel">

                <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-4">

                    <div>
                        <h5 class="mb-1">Historial de stock</h5>

                        <p class="text-muted small mb-0">
                            ${escapeHtml(autopart?.codigo || "")} -
                            ${escapeHtml(autopart?.nombre || "")}
                        </p>
                    </div>

                    <button class="btn btn-outline-secondary btn-sm"
                            id="backFromStockHistory">
                        <i class="bi bi-arrow-left me-2"></i>Volver
                    </button>

                </div>

                <div class="table-responsive">

                    <table class="table dashboard-table align-middle">

                        <thead>
                            <tr>
                                <th>Fecha</th>
                                <th>Movimiento</th>
                                <th>Cantidad</th>
                                <th>Stock resultante</th>
                                <th>Factura</th>
                                <th>Orden de reposición</th>
                                <th>Observaciones</th>
                            </tr>
                        </thead>

                        <tbody>

                            ${records.length === 0 ? `
                                <tr>
                                    <td colspan="7"
                                        class="text-center text-muted py-4">
                                        No hay movimientos registrados para esta autoparte.
                                    </td>
                                </tr>
                            ` : records.map(record => {

            const isEntry =
                record.tipoMovimiento === "E";

            const movementLabel = isEntry
                ? "Entrada"
                : record.tipoMovimiento === "S"
                    ? "Salida"
                    : "Desconocido";

            const badgeClass = isEntry
                ? "bg-success"
                : record.tipoMovimiento === "S"
                    ? "bg-danger"
                    : "bg-secondary";

            return `
                                    <tr>

                                        <td>
                                            ${formatDate(record.fechaMovimiento)}
                                        </td>

                                        <td>
                                            <span class="badge ${badgeClass}">
                                                ${movementLabel}
                                            </span>
                                        </td>

                                        <td>
                                            ${record.cantidad ?? 0}
                                        </td>

                                        <td>
                                            <strong>
                                                ${record.stockResultante ?? 0}
                                            </strong>
                                        </td>

                                        <td>
                                            ${record.facturaId ?? "-"}
                                        </td>

                                        <td>
                                            ${record.ordenReposicionId ?? "-"}
                                        </td>

                                        <td>
                                            ${escapeHtml(record.observaciones || "-")}
                                        </td>

                                    </tr>
                                `;

        }).join("")}

                        </tbody>

                    </table>

                </div>

                <div class="text-muted small mt-2">
                    ${records.length} movimientos encontrados.
                </div>

            </div>
        `;

        document
            .getElementById("backFromStockHistory")
            .addEventListener("click", renderAutopartsModule);

    } catch (error) {

        console.error("Error al consultar historial de stock:", error);

        moduleContent.innerHTML = `
            <div class="alert alert-danger">

                <p>${escapeHtml(error.message)}</p>

                <button class="btn btn-outline-secondary btn-sm"
                        id="backFromStockHistory">
                    Volver
                </button>

            </div>
        `;

        document
            .getElementById("backFromStockHistory")
            .addEventListener("click", renderAutopartsModule);
    }
}

window.changeAutopartStatus = async function changeAutopartStatus(id) {

    const autopart = autopartsCache.find(
        item => item.autoparteId === id
    );

    if (!autopart) {
        alert("No se encontró la autoparte seleccionada.");
        return;
    }

    const newStatus = !autopart.estado;

    const action = newStatus
        ? "reactivar"
        : "dar de baja";

    const confirmed = confirm(
        `¿Estás seguro de que querés ${action} la autoparte "${autopart.nombre}"?`
    );

    if (!confirmed) {
        return;
    }

    try {

        await apiRequest(
            `/api/Autoparte/${id}/estado`,
            {
                method: "PATCH",
                body: JSON.stringify(newStatus)
            }
        );

        await loadAutoparts();

        showModuleSuccess(
            newStatus
                ? "Autoparte reactivada correctamente."
                : "Autoparte dada de baja correctamente."
        );

    } catch (error) {

        alert(error.message);
    }
}

