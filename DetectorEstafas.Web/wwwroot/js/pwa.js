"use strict";

let deferredInstallPrompt = null;

const installButton =
    document.getElementById("installAppButton");

function estaInstalada() {
    return (
        window.matchMedia("(display-mode: standalone)").matches ||
        window.navigator.standalone === true
    );
}

function esIos() {
    return (
        /iPad|iPhone|iPod/.test(navigator.userAgent) ||
        (
            navigator.platform === "MacIntel" &&
            navigator.maxTouchPoints > 1
        )
    );
}

function hideInstallButton() {
    if (installButton) {
        installButton.hidden = true;
    }
}

function showInstallButton() {
    if (installButton) {
        installButton.hidden = false;
    }
}

if ("serviceWorker" in navigator) {
    window.addEventListener("load", async () => {
        try {
            const registration = await navigator.serviceWorker.register(
                "/service-worker.js",
                {
                    scope: "/",
                    updateViaCache: "none"
                });

            await registration.update();
        }
        catch (error) {
            console.error(
                "No se pudo configurar la PWA.",
                error);
        }
    });
}

window.addEventListener(
    "beforeinstallprompt",
    event => {
        event.preventDefault();

        deferredInstallPrompt = event;
    });

if (installButton) {

    installButton.addEventListener(
        "click",
        async () => {

            if (estaInstalada()) {
                hideInstallButton();
                return;
            }

            if (deferredInstallPrompt) {
                deferredInstallPrompt.prompt();

                await deferredInstallPrompt.userChoice;

                deferredInstallPrompt = null;

                return;
            }

            if (esIos()) {
                alert(
                    "Para instalar AlertaEstafa en iPhone o iPad: " +
                    "tocá Compartir y luego «Añadir a pantalla de inicio»."
                );
                return;
            }

            alert(
                "Para instalar AlertaEstafa, usá la opción de instalación " +
                "del navegador cuando esté disponible."
            );
        });
}

window.addEventListener(
    "appinstalled",
    () => {
        deferredInstallPrompt = null;
        hideInstallButton();
    });

function actualizarModoInstalado() {
    const instalada = estaInstalada();

    document.documentElement.classList.toggle(
        "pwa-standalone",
        instalada);

    if (instalada) {
        hideInstallButton();
    }
    else {
        showInstallButton();
    }
}

actualizarModoInstalado();

window.matchMedia("(display-mode: standalone)")
    .addEventListener?.(
        "change",
        actualizarModoInstalado);
