// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


//Polling , por segundos pido la data, uso en el caht , lista de mensajes y numero del menu.
function actualizarCada(caja, segundos, opciones = {}) {
    if (!caja) return;

    setInterval(async () => {
        //pestaña escondida
        if (document.hidden) return;

        try {
            const r = await fetch(caja.dataset.url, { cache: 'no-store' });

            //redirected
            if (!r.ok || r.redirected) return;

            const html = await r.text();
            const guardado = opciones.antes ? opciones.antes() : null;
            caja.innerHTML = html;
            if (opciones.despues) opciones.despues(guardado);
        } catch {
            //sin conexion
        }
    }, segundos * 1000);
}