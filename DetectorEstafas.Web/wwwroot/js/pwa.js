"use strict";

let deferredInstallPrompt = null;

const installButton =
    document.getElementById("installAppButton");

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

async function limpiarPwaEnDesarrollo() {
    const esLocal =
        window.location.hostname === "localhost" ||
        window.location.hostname === "127.0.0.1";

    if (!esLocal) {
        return false;
    }

    if ("serviceWorker" in navigator) {
        const registros =
            await navigator.serviceWorker.getRegistrations();

        for (const registro of registros) {
            await registro.unregister();
        }
    }

    if ("caches" in window) {
        const nombresCache = await caches.keys();

        await Promise.all(
            nombresCache.map(nombre =>
                caches.delete(nombre)));
    }

    return true;
}

if ("serviceWorker" in navigator) {
    window.addEventListener("load", async () => {
        try {
            const desarrollo =
                await limpiarPwaEnDesarrollo();

            if (desarrollo) {
                return;
            }

            await navigator.serviceWorker.register(
                "/service-worker.js",
                {
                    scope: "/"
                });
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

        showInstallButton();
    });

if (installButton) {
    installButton.addEventListener(
        "click",
        async () => {
            if (!deferredInstallPrompt) {
                return;
            }

            deferredInstallPrompt.prompt();

            await deferredInstallPrompt.userChoice;

            deferredInstallPrompt = null;

            hideInstallButton();
        });
}

window.addEventListener(
    "appinstalled",
    () => {
        deferredInstallPrompt = null;

        hideInstallButton();
    });
function actualizarModoInstalado() {
    const instalada =
        window.matchMedia("(display-mode: standalone)").matches ||
        window.navigator.standalone === true;

    document.documentElement.classList.toggle(
        "pwa-standalone",
        instalada);
}

actualizarModoInstalado();

window.matchMedia("(display-mode: standalone)")
    .addEventListener?.("change", actualizarModoInstalado);
