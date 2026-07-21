(function () {
  'use strict';

  const $ = (sel, ctx) => (ctx || document).querySelector(sel);
  const $$ = (sel, ctx) => [...(ctx || document).querySelectorAll(sel)];

  /* Splash */
  const splash = $('#m26-splash');
  if (splash) {
    window.addEventListener('load', () => {
      setTimeout(() => splash.classList.add('is-done'), 600);
    });
  }

  /* Header scroll */
  const head = $('#m26-head');
  if (head) {
    const onScroll = () => head.classList.toggle('is-scrolled', window.scrollY > 20);
    window.addEventListener('scroll', onScroll, { passive: true });
    onScroll();
  }

  /* Drawer */
  const drawer = $('#m26-drawer');
  const openDrawer = () => {
    drawer?.classList.add('is-open');
    document.body.style.overflow = 'hidden';
  };
  const closeDrawer = () => {
    drawer?.classList.remove('is-open');
    document.body.style.overflow = '';
  };
  $('#m26-burger')?.addEventListener('click', openDrawer);
  $('#m26-bnav-menu')?.addEventListener('click', openDrawer);
  $$('[data-close]', drawer).forEach(el => el.addEventListener('click', closeDrawer));
  drawer?.addEventListener('click', e => { if (e.target === drawer) closeDrawer(); });

  /* Search */
  const search = $('#m26-search');
  const searchBtn = $('#m26-search-btn');
  const searchInput = $('#m26-search-input');
  searchBtn?.addEventListener('click', () => {
    search?.classList.toggle('is-open');
    searchBtn?.setAttribute('aria-expanded', search?.classList.contains('is-open'));
    if (search?.classList.contains('is-open')) searchInput?.focus();
  });
  document.addEventListener('click', e => {
    if (search && !search.contains(e.target)) {
      search.classList.remove('is-open');
      searchBtn?.setAttribute('aria-expanded', 'false');
    }
  });

  /* Nav overflow "more" */
  const nav = $('.m26-nav');
  const moreWrap = $('#m26-more');
  const moreList = $('#m26-more-list');
  const moreBtn = $('.m26-more-btn');

  function fitNav() {
    if (!nav || !moreWrap || window.innerWidth <= 1024) {
      moreWrap?.setAttribute('hidden', '');
      return;
    }
    moreWrap.removeAttribute('hidden');
    $$('.m26-nav > li.is-overflow').forEach(li => {
      li.classList.remove('is-overflow');
      nav.appendChild(li);
    });
    moreList.innerHTML = '';
    const items = $$('.m26-nav > li');
    for (let i = items.length - 1; i >= 0; i--) {
      if (nav.scrollWidth <= nav.clientWidth) break;
      const li = items[i];
      if (li.closest('#m26-more')) continue;
      li.classList.add('is-overflow');
      const clone = li.cloneNode(true);
      moreList.prepend(clone);
      li.remove();
    }
    if (!moreList.children.length) moreWrap.setAttribute('hidden', '');
  }
  moreBtn?.addEventListener('click', () => moreWrap?.classList.toggle('is-open'));
  window.addEventListener('resize', fitNav);
  fitNav();

  /* Hero spotlight */
  const spot = $('#m26-spot');
  const hero = $('#m26-hero');
  if (spot && hero) {
    hero.addEventListener('mousemove', e => {
      const r = hero.getBoundingClientRect();
      spot.style.left = ((e.clientX - r.left) / r.width * 100) + '%';
      spot.style.top = ((e.clientY - r.top) / r.height * 100) + '%';
    });
  }

  /* Reveal on scroll */
  const revealEls = $$('.reveal');
  if (revealEls.length && 'IntersectionObserver' in window) {
    const obs = new IntersectionObserver(entries => {
      entries.forEach(en => {
        if (en.isIntersecting) {
          en.target.classList.add('is-visible');
          obs.unobserve(en.target);
        }
      });
    }, { threshold: 0.12, rootMargin: '0px 0px -40px 0px' });
    revealEls.forEach(el => obs.observe(el));
  } else {
    revealEls.forEach(el => el.classList.add('is-visible'));
  }

  /* Counter animation */
  const counters = $$('[data-count]');
  if (counters.length && 'IntersectionObserver' in window) {
    const countObs = new IntersectionObserver(entries => {
      entries.forEach(en => {
        if (!en.isIntersecting) return;
        const el = en.target;
        const target = parseInt(el.dataset.count, 10);
        const suffix = el.dataset.suffix || '';
        const duration = 1800;
        const start = performance.now();
        const tick = now => {
          const p = Math.min((now - start) / duration, 1);
          const eased = 1 - Math.pow(1 - p, 3);
          el.textContent = Math.round(target * eased) + suffix;
          if (p < 1) requestAnimationFrame(tick);
        };
        requestAnimationFrame(tick);
        countObs.unobserve(el);
      });
    }, { threshold: 0.5 });
    counters.forEach(c => countObs.observe(c));
  }

  /* Back to top */
  const topBtn = $('#m26-top');
  if (topBtn) {
    window.addEventListener('scroll', () => {
      topBtn.classList.toggle('is-visible', window.scrollY > 400);
    }, { passive: true });
    topBtn.addEventListener('click', () => window.scrollTo({ top: 0, behavior: 'smooth' }));
  }

  /* Vietnamese date in utility bar */
  const dateEl = $('#m26-date');
  if (dateEl) {
    const days = ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'];
    const now = new Date();
    const pad = n => String(n).padStart(2, '0');
    dateEl.textContent = `${days[now.getDay()]}, ${pad(now.getDate())}/${pad(now.getMonth() + 1)}/${now.getFullYear()} - ${pad(now.getHours())}:${pad(now.getMinutes())}`;
  }
})();
