(function () {
  var reducir = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  var hayObservador = "IntersectionObserver" in window;

  var nav = document.querySelector(".nav");
  var botonMenu = document.querySelector(".nav-boton");
  var menu = document.getElementById("nav-menu");

  function cerrarMenu() {
    nav.removeAttribute("data-abierto");
    botonMenu.setAttribute("aria-expanded", "false");
    botonMenu.setAttribute("aria-label", "Abrir menú");
  }

  if (nav && botonMenu && menu) {
    botonMenu.addEventListener("click", function () {
      if (nav.hasAttribute("data-abierto")) {
        cerrarMenu();
        return;
      }
      nav.setAttribute("data-abierto", "");
      botonMenu.setAttribute("aria-expanded", "true");
      botonMenu.setAttribute("aria-label", "Cerrar menú");
    });

    menu.addEventListener("click", function (e) {
      if (e.target.closest("a")) cerrarMenu();
    });

    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape" && nav.hasAttribute("data-abierto")) {
        cerrarMenu();
        botonMenu.focus();
      }
    });
  }

  var revelar = document.querySelectorAll(".revelar");
  if (reducir || !hayObservador) {
    revelar.forEach(function (el) { el.classList.add("visible"); });
  } else {
    var observarRevelar = new IntersectionObserver(function (entradas) {
      entradas.forEach(function (entrada) {
        if (entrada.isIntersecting) {
          entrada.target.classList.add("visible");
          observarRevelar.unobserve(entrada.target);
        }
      });
    }, { threshold: 0.12, rootMargin: "0px 0px -40px 0px" });
    revelar.forEach(function (el) { observarRevelar.observe(el); });
  }

  var enlaces = document.querySelectorAll('.nav-menu a[href^="#"]');
  if (hayObservador && enlaces.length) {
    var enlacePorSeccion = {};
    enlaces.forEach(function (a) { enlacePorSeccion[a.getAttribute("href").slice(1)] = a; });
    var observarSecciones = new IntersectionObserver(function (entradas) {
      entradas.forEach(function (entrada) {
        var enlace = enlacePorSeccion[entrada.target.id];
        if (!enlace || !entrada.isIntersecting) return;
        enlaces.forEach(function (a) { a.classList.remove("activo"); });
        enlace.classList.add("activo");
      });
    }, { rootMargin: "-45% 0px -50% 0px" });
    Object.keys(enlacePorSeccion).forEach(function (id) {
      var seccion = document.getElementById(id);
      if (seccion) observarSecciones.observe(seccion);
    });
  }

  var pestanas = Array.prototype.slice.call(document.querySelectorAll('.pestanas [role="tab"]'));
  var panel = document.getElementById("panel-recorrido");
  var imgRecorrido = document.getElementById("recorrido-img");
  var textoRecorrido = document.getElementById("recorrido-texto");

  function elegir(pestana, enfocar) {
    pestanas.forEach(function (p) {
      var activa = p === pestana;
      p.setAttribute("aria-selected", activa ? "true" : "false");
      p.tabIndex = activa ? 0 : -1;
    });
    panel.setAttribute("aria-labelledby", pestana.id);
    textoRecorrido.textContent = pestana.dataset.texto;

    var nueva = pestana.dataset.img;
    if (imgRecorrido.getAttribute("src") !== nueva) {
      imgRecorrido.classList.add("cambiando");
      var precarga = new Image();
      precarga.onload = precarga.onerror = function () {
        imgRecorrido.src = nueva;
        imgRecorrido.alt = "Sección " + pestana.textContent + " de LyXel";
        imgRecorrido.classList.remove("cambiando");
      };
      precarga.src = nueva;
    }

    if (enfocar) pestana.focus();
    var barra = pestana.parentElement;
    barra.scrollTo({
      left: pestana.offsetLeft - (barra.clientWidth - pestana.clientWidth) / 2,
      behavior: reducir ? "auto" : "smooth"
    });
  }

  if (pestanas.length && panel && imgRecorrido && textoRecorrido) {
    pestanas.forEach(function (pestana, i) {
      pestana.addEventListener("click", function () { elegir(pestana, false); });
      pestana.addEventListener("keydown", function (e) {
        var destino = null;
        if (e.key === "ArrowRight") destino = pestanas[(i + 1) % pestanas.length];
        else if (e.key === "ArrowLeft") destino = pestanas[(i - 1 + pestanas.length) % pestanas.length];
        else if (e.key === "Home") destino = pestanas[0];
        else if (e.key === "End") destino = pestanas[pestanas.length - 1];
        if (destino) {
          e.preventDefault();
          elegir(destino, true);
        }
      });
    });

    var recorrido = document.getElementById("recorrido");
    if (hayObservador && recorrido) {
      var observarRecorrido = new IntersectionObserver(function (entradas) {
        if (!entradas[0].isIntersecting) return;
        pestanas.forEach(function (p) { new Image().src = p.dataset.img; });
        observarRecorrido.disconnect();
      }, { rootMargin: "400px 0px" });
      observarRecorrido.observe(recorrido);
    }
  }

  var cifra = document.getElementById("cifra-descargas");
  if (cifra && window.fetch) {
    var control = "AbortController" in window ? new AbortController() : null;
    var limite = setTimeout(function () { if (control) control.abort(); }, 5000);
    fetch("https://api.github.com/repos/sudoNks/Lyxel/releases?per_page=100", { signal: control ? control.signal : undefined })
      .then(function (r) {
        if (!r.ok) throw new Error(r.status);
        return r.json();
      })
      .then(function (versiones) {
        var total = 0;
        versiones.forEach(function (v) {
          (v.assets || []).forEach(function (a) { total += a.download_count || 0; });
        });
        if (total >= 1000) cifra.textContent = "+" + Math.floor(total / 1000) + " mil";
        else if (total > 0) cifra.textContent = "+" + total;
      })
      .catch(function () {})
      .then(function () { clearTimeout(limite); });
  }

  var visor = document.getElementById("visor");
  var visorImg = document.getElementById("visor-img");
  if (visor && visorImg && typeof visor.showModal === "function") {
    document.addEventListener("click", function (e) {
      var img = e.target.closest(".ventana img");
      if (!img || img.closest(".ventana-idiomas")) return;
      visorImg.src = img.currentSrc || img.src;
      visorImg.alt = img.alt;
      visor.showModal();
    });
    visor.addEventListener("click", function () { visor.close(); });
    visor.addEventListener("close", function () { visorImg.removeAttribute("src"); });
  }

  var anio = document.getElementById("anio");
  if (anio) anio.textContent = new Date().getFullYear();
})();
