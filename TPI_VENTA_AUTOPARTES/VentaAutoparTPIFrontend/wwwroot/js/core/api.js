
window.refreshAccessToken = async function refreshAccessToken() {

    if (refreshPromise) {
        return refreshPromise;
    }

    refreshPromise = (async () => {

        const token = sessionStorage.getItem("refreshToken");

        if (!token) {
            throw new Error("No existe un refresh token disponible");
        }

        const response = await fetch(
            `${API_BASE_URL}/api/Auth/refresh`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
                body: JSON.stringify({
                    refreshToken: token
                })
            }
        );

        const responseText = await response.text();
        let data = null;

        if (responseText) {
            try {
                data = JSON.parse(responseText);
            } catch {
                throw new Error("La API devolvió una respuesta no válida");
            }
        }

        if (!response.ok) {
            throw new Error(
                data?.message ||
                "No se pudo renovar la sesión"
            );
        }

        const newAccessToken = data?.accessToken;
        const newRefreshToken = data?.refreshToken;

        if (!newAccessToken || !newRefreshToken) {
            throw new Error(
                "La API no devolvió los nuevos tokens de autenticación"
            );
        }

        currentAccessToken = newAccessToken;
        currentRefreshToken = newRefreshToken;

        sessionStorage.setItem("accessToken", newAccessToken);
        sessionStorage.setItem("refreshToken", newRefreshToken);

        if (data.accessTokenExpiration) {
            sessionStorage.setItem(
                "accessTokenExpiration",
                data.accessTokenExpiration
            );
        }

        return newAccessToken;

    })();

    try {
        return await refreshPromise;
    } finally {
        refreshPromise = null;
    }
}

window.processApiResponse = async function processApiResponse(response) {

    const responseText = await response.text();
    let data = null;

    if (responseText) {
        try {
            data = JSON.parse(responseText);
        } catch {
            throw new Error("La API devolvió una respuesta no válida");
        }
    }

    if (!response.ok) {
        throw new Error(
            data?.message ||
            data?.title ||
            "No se pudo completar la operación"
        );
    }

    return data;
}

window.apiRequest = async function apiRequest(endpoint, options = {}) {

    const requestOptions = {
        ...options,
        headers: {
            "Accept": "application/json",
            ...(options.body !== undefined
                ? { "Content-Type": "application/json" }
                : {}),
            ...(options.headers || {}),
            "Authorization": `Bearer ${currentAccessToken}`
        }
    };

    let response = await fetch(
        `${API_BASE_URL}${endpoint}`,
        requestOptions
    );

    if (response.status === 401) {

        try {

            await refreshAccessToken();

            requestOptions.headers.Authorization =
                `Bearer ${currentAccessToken}`;

            response = await fetch(
                `${API_BASE_URL}${endpoint}`,
                requestOptions
            );

        } catch (error) {

            console.error("No se pudo renovar la sesión:", error);

            clearSession();
            window.location.replace("/pages/login.html");

            throw new Error(
                "La sesión expiró. Iniciá sesión nuevamente"
            );
        }
    }

    if (response.status === 401) {

        clearSession();
        window.location.replace("/pages/login.html");

        throw new Error(
            "La sesión expiró. Iniciá sesión nuevamente"
        );
    }

    return await processApiResponse(response);
}
