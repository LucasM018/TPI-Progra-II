
window.renderAboutModule = function renderAboutModule() {

    moduleContent.innerHTML = `

        <div class="autoparts-panel">

            <!-- Presentación del sistema -->

            <div class="text-center py-4 mb-4">

                <div class="mb-3">
                    <i class="bi bi-car-front-fill"
                       style="font-size: 3.5rem; color: var(--accent);">
                    </i>
                </div>

                <h3 class="fw-bold mb-2">AutoGestock</h3>

                <h5 class="text-muted mb-3">
                    Tu negocio bajo control.
                </h5>

                <p class="text-muted mx-auto"
                   style="max-width: 750px;">

                    AutoGestock es un sistema de gestión diseñado
                    para administrar la venta de autopartes y
                    controlar el stock de manera eficiente.

                    Permite gestionar productos, clientes, facturas,
                    movimientos de inventario y consultar indicadores
                    relevantes para la toma de decisiones.

                </p>

            </div>

            <hr class="my-4">

            <!-- Equipo de desarrollo -->

            <div class="mb-4">

                <div class="text-center mb-4">

                    <h5 class="fw-bold">Responsables del desarrollo</h5>

                    <p class="text-muted small">
                        Equipo responsable del desarrollo de AutoGestock.
                    </p>

                </div>

                <div class="row g-4 justify-content-center">

                    <!-- Taddeo -->

                    <div class="col-md-6 col-lg-4">

                        <div class="card h-100 border-0 shadow-sm">

                            <div class="card-body text-center p-4">

                                <img
                                    src="/images/integrantes/taddeo.jpg"
                                    alt="Foto de Taddeo Andrada Righini"
                                    class="rounded-circle mb-3"
                                    style="
                                        width: 150px;
                                        height: 150px;
                                        object-fit: cover;
                                    "
                                    onerror="this.style.display='none';"
                                >

                                <h6 class="fw-bold mb-1">
                                    Taddeo Andrada Righini
                                </h6>

                                <p class="text-muted small mb-2">
                                    Legajo: 430198
                                </p>

                                <span class="badge bg-primary">
                                    Integrante de AutoGestock
                                </span>

                            </div>

                        </div>

                    </div>

                    <!-- Jesús -->

                    <div class="col-md-6 col-lg-4">

                        <div class="card h-100 border-0 shadow-sm">

                            <div class="card-body text-center p-4">

                                <img
                                    src="/images/integrantes/jesus.jpg"
                                    alt="Foto de Jesus Ruben Oviedo"
                                    class="rounded-circle mb-3"
                                    style="
                                        width: 150px;
                                        height: 150px;
                                        object-fit: cover;
                                    "
                                    onerror="this.style.display='none';"
                                >

                                <h6 class="fw-bold mb-1">
                                    Jesus Ruben Oviedo
                                </h6>

                                <p class="text-muted small mb-2">
                                    Legajo: 421556
                                </p>

                                <span class="badge bg-primary">
                                    Integrante de AutoGestock
                                </span>

                            </div>

                        </div>

                    </div>

                    <!-- Lucas -->

                    <div class="col-md-6 col-lg-4">

                        <div class="card h-100 border-0 shadow-sm">

                            <div class="card-body text-center p-4">

                                <img
                                    src="/images/integrantes/lucas.png"
                                    alt="Foto de Mendoza Lucas Gabriel"
                                    class="rounded-circle mb-3"
                                    style="
                                        width: 150px;
                                        height: 150px;
                                        object-fit: cover;
                                    "
                                    onerror="this.style.display='none';"
                                >

                                <h6 class="fw-bold mb-1">
                                    Mendoza Lucas Gabriel
                                </h6>

                                <p class="text-muted small mb-2">
                                    Legajo: 421267
                                </p>

                                <span class="badge bg-primary">
                                    Integrante de AutoGestock
                                </span>

                            </div>

                        </div>

                    </div>

                </div>

            </div>

            <hr class="my-4">

            <!-- Tecnologías utilizadas -->

            <div class="text-center py-3">

                <h5 class="fw-bold mb-3">
                    Tecnologías utilizadas
                </h5>

                <div class="d-flex justify-content-center flex-wrap gap-2">

                    <span class="badge bg-secondary p-2">C#</span>

                    <span class="badge bg-secondary p-2">.NET 8</span>

                    <span class="badge bg-secondary p-2">
                        Entity Framework Core
                    </span>

                    <span class="badge bg-secondary p-2">
                        SQL Server
                    </span>

                    <span class="badge bg-secondary p-2">HTML</span>

                    <span class="badge bg-secondary p-2">CSS</span>

                    <span class="badge bg-secondary p-2">Bootstrap</span>

                    <span class="badge bg-secondary p-2">JavaScript</span>

                </div>

            </div>

        </div>
    `;
}
