"use strict";

document.addEventListener("DOMContentLoaded", () => {
    configurarCambioTipoContenido();
    mostrarErrorDeCarga();
});

function configurarCambioTipoContenido() {
    const tipo = document.getElementById("Tipo");
    const contenido = document.getElementById("Contenido");

    if (!tipo || !contenido) {
        return;
    }

    tipo.addEventListener("change", () => {
        contenido.value = "";

        const validation = document.querySelector(
            '[data-valmsg-for="Contenido"]');

        if (validation) {
            validation.textContent = "";
            validation.classList.remove("field-validation-error");
            validation.classList.add("field-validation-valid");
        }

        contenido.focus();
    });
}

function mostrarErrorDeCarga() {
    const error = document.querySelector(".capture-error");

    if (!error) {
        return;
    }

    error.setAttribute("tabindex", "-1");

    window.requestAnimationFrame(() => {
        error.scrollIntoView({
            behavior: "smooth",
            block: "center"
        });

        error.focus({ preventScroll: true });
    });
}
