
const API_BASE_URL = "https://localhost:7086";

// Obtener los elementos del formulario
const loginForm = document.getElementById("loginForm");
const emailInput = document.getElementById("email");
const passwordInput = document.getElementById("password");
const loginButton = document.getElementById("loginButton");
const loginMessage = document.getElementById("loginMessage");
const togglePassword = document.getElementById("togglePassword");

// Mostrar u ocultar la contraseña
togglePassword.addEventListener("click", () => {
    const isPassword = passwordInput.type === "password";

    passwordInput.type = isPassword ? "text" : "password";

    togglePassword.innerHTML = isPassword
        ? '<i class="bi bi-eye-slash"></i>'
        : '<i class="bi bi-eye"></i>';

    togglePassword.setAttribute(
        "aria-label",
        isPassword ? "Ocultar contraseña" : "Mostrar contraseña"
    );
});

function showMessage(message, type) {
    loginMessage.textContent = message;
    loginMessage.className = `login-message ${type}`;
}

function clearMessage() {
    loginMessage.textContent = "";
    loginMessage.className = "login-message";
}

loginForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    clearMessage();

    const email = emailInput.value.trim();
    const password = passwordInput.value;

    if (!email || !password) {
        showMessage(
            "Completá el correo electrónico y la contraseña.",
            "error"
        );
        return;
    }

    loginButton.disabled = true;
    loginButton.querySelector("span").textContent = "VALIDANDO...";
    loginButton.querySelector("i").className = "bi bi-hourglass-split";

    try {
        const response = await fetch(`${API_BASE_URL}/api/Auth/login`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                email: email,
                password: password
            })
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(
                data.message || "No se pudo iniciar sesión."
            );
        }

        if (!data.accessToken || !data.refreshToken) {
            throw new Error(
                "La respuesta de autenticación no contiene los tokens esperados."
            );
        }

        sessionStorage.setItem("accessToken", data.accessToken);
        sessionStorage.setItem("refreshToken", data.refreshToken);

        sessionStorage.setItem(
            "accessTokenExpiration",
            data.accessTokenExpiration
        );

        const user = {
            usuarioId: data.usuarioId,
            nombre: data.nombre,
            apellido: data.apellido,
            email: data.email,
            roles: data.roles || []
        };

        sessionStorage.setItem("currentUser", JSON.stringify(user));

        showMessage(
            "Inicio de sesión exitoso. Redirigiendo...",
            "success"
        );

        window.location.href = "/pages/dashboard.html";

    } catch (error) {
        console.error("Error de autenticación:", error);

        sessionStorage.removeItem("accessToken");
        sessionStorage.removeItem("refreshToken");
        sessionStorage.removeItem("accessTokenExpiration");
        sessionStorage.removeItem("currentUser");

        if (error instanceof TypeError) {
            showMessage(
                "No se pudo conectar con el servidor. Verificá que la API esté funcionando y que el certificado HTTPS sea válido.",
                "error"
            );
        } else {
            showMessage(error.message, "error");
        }

    } finally {
        loginButton.disabled = false;
        loginButton.querySelector("span").textContent = "INGRESAR AL SISTEMA";
        loginButton.querySelector("i").className = "bi bi-arrow-right";
    }
});
