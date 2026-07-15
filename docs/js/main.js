// LyXel — Lógica del sitio (vanilla JS, sin dependencias)
// Este archivo agrupa: 1) contador de descargas vía API pública de GitHub
// y 2) revelado progresivo de secciones al hacer scroll.

// Contador de descargas en vivo desde la API pública de GitHub (con respaldo si falla)
(function () {
  try {
    var textoEl = document.getElementById('hero-badge-text');
    if (!textoEl) return;
    var textoRespaldo = textoEl.textContent;
    var controlador = ('AbortController' in window) ? new AbortController() : null;
    var limiteTiempo = setTimeout(function () {
      if (controlador) controlador.abort();
    }, 4000);

    fetch('https://api.github.com/repos/sudoNks/Lyxel/releases', {
      signal: controlador ? controlador.signal : undefined
    })
      .then(function (respuesta) {
        if (!respuesta.ok) throw new Error('Respuesta no válida de la API de GitHub');
        return respuesta.json();
      })
      .then(function (releases) {
        var total = 0;
        (releases || []).forEach(function (release) {
          (release.assets || []).forEach(function (asset) {
            total += asset.download_count || 0;
          });
        });
        if (total > 0) {
          textoEl.textContent = '\uD83D\uDD3D +' + total.toLocaleString('es-ES') + ' descargas';
        }
      })
      .catch(function () {
        textoEl.textContent = textoRespaldo;
      })
      .finally(function () {
        clearTimeout(limiteTiempo);
      });
  } catch (error) {
    // Cualquier fallo inesperado deja el texto de respaldo tal como está en el HTML
  }
})();
// Revelado progresivo de secciones al hacer scroll (respeta reduce-motion)
(function () {
  const elementos = document.querySelectorAll('.reveal');
  const prefiereMenosMovimiento = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  if (prefiereMenosMovimiento || !('IntersectionObserver' in window)) {
    elementos.forEach(function (el) { el.classList.add('is-visible'); });
    return;
  }

  const observador = new IntersectionObserver(function (entradas) {
    entradas.forEach(function (entrada) {
      if (entrada.isIntersecting) {
        entrada.target.classList.add('is-visible');
        observador.unobserve(entrada.target);
      }
    });
  }, { threshold: 0.15 });

  elementos.forEach(function (el) { observador.observe(el); });
})();
