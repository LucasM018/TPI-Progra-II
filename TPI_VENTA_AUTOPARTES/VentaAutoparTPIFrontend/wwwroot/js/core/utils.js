
window.showAutopartMessage = function showAutopartMessage(element, message, type) {

    element.textContent = message;
    element.className = `autoparts-message ${type}`;
}

window.showModuleSuccess = function showModuleSuccess(message) {

    const alertElement = document.createElement("div");

    alertElement.className = "alert alert-success mb-3";
    alertElement.textContent = message;

    moduleContent.prepend(alertElement);
}

window.formatDate = function formatDate(value) {

    if (!value) {
        return "-";
    }

    const hasTimeZone = /Z$|[+-]\d{2}:\d{2}$/.test(value);

    const date = new Date(
        hasTimeZone ? value : `${value}Z`
    );

    if (Number.isNaN(date.getTime())) {
        return "-";
    }

    return new Intl.DateTimeFormat("es-AR", {
        timeZone: "America/Argentina/Buenos_Aires",
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
        hourCycle: "h23"
    }).format(date);
};

window.escapeHtml = function escapeHtml(value) {

    return String(value ?? "").replace(/[&<>"']/g, character => {

        const entities = {
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            '"': "&quot;",
            "'": "&#39;"
        };

        return entities[character];
    });
}
