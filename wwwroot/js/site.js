/* ── Auto-dismiss toasts after 4s ──────────────────────────── */
document.querySelectorAll('.toast').forEach(function (t) {
    setTimeout(function () {
        t.style.opacity = '0';
        t.style.transition = 'opacity .4s';
        setTimeout(function () { t.remove(); }, 400);
    }, 4000);
});

/* ── Mobile sidebar (hamburger menu) ──────────────────────────
   Injects the toggle button + overlay into the DOM automatically,
   so NO view changes are needed at all.
────────────────────────────────────────────────────────────── */
(function () {
    var sidebar  = document.querySelector('.sidebar');
    var mainContent = document.querySelector('.main-content');
    if (!sidebar) return;   // not on authenticated pages

    /* Create hamburger button */
    var btn = document.createElement('button');
    btn.className   = 'menu-toggle';
    btn.setAttribute('aria-label', 'Abrir menú');
    btn.setAttribute('aria-expanded', 'false');
    btn.innerHTML   =
        '<span></span><span></span><span></span>';
    document.body.appendChild(btn);

    /* Create overlay */
    var overlay = document.createElement('div');
    overlay.className = 'sidebar-overlay';
    document.body.appendChild(overlay);

    function openSidebar() {
        sidebar.classList.add('open');
        overlay.classList.add('open');
        btn.classList.add('open');
        btn.setAttribute('aria-expanded', 'true');
        document.body.style.overflow = 'hidden';
    }

    function closeSidebar() {
        sidebar.classList.remove('open');
        overlay.classList.remove('open');
        btn.classList.remove('open');
        btn.setAttribute('aria-expanded', 'false');
        document.body.style.overflow = '';
    }

    btn.addEventListener('click', function () {
        if (sidebar.classList.contains('open')) {
            closeSidebar();
        } else {
            openSidebar();
        }
    });

    overlay.addEventListener('click', closeSidebar);

    /* Close on nav-item tap (SPA-like feel) */
    sidebar.querySelectorAll('.nav-item').forEach(function (link) {
        link.addEventListener('click', function () {
            if (window.innerWidth <= 768) closeSidebar();
        });
    });

    /* Close on resize back to desktop */
    window.addEventListener('resize', function () {
        if (window.innerWidth > 768) {
            closeSidebar();
            document.body.style.overflow = '';
        }
    });

    /* Trap focus inside sidebar when open (accessibility) */
    sidebar.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') closeSidebar();
    });
})();
