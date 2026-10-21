(() => {
    const $ = (selector, root = document) => root.querySelector(selector);
    const $$ = (selector, root = document) => [...root.querySelectorAll(selector)];

    const navToggle = $('[data-nav-toggle]');
    const adminNav = $('[data-admin-nav]');
    navToggle?.addEventListener('click', () => {
        const open = adminNav.classList.toggle('is-open');
        navToggle.setAttribute('aria-expanded', open);
    });

    const toastRegion = $('[data-toast-region]');
    let toastTimer;
    const showToast = message => {
        if (!toastRegion) return;
        toastRegion.textContent = message;
        toastRegion.classList.add('is-visible');
        clearTimeout(toastTimer);
        toastTimer = setTimeout(() => toastRegion.classList.remove('is-visible'), 2800);
    };
    $$('[data-toast]').forEach(button => button.addEventListener('click', () => showToast(button.dataset.toast)));

    $$('[data-drawer-open]').forEach(button => button.addEventListener('click', () => {
        const drawer = document.getElementById(button.dataset.drawerOpen);
        drawer?.classList.add('is-open');
        $('.drawer-backdrop')?.classList.add('is-open');
        document.body.style.overflow = 'hidden';
    }));
    const closeDrawers = () => {
        $$('.drawer.is-open').forEach(drawer => drawer.classList.remove('is-open'));
        $('.drawer-backdrop')?.classList.remove('is-open');
        document.body.style.overflow = '';
    };
    $$('[data-drawer-close]').forEach(button => button.addEventListener('click', closeDrawers));
    $('.drawer-backdrop')?.addEventListener('click', closeDrawers);
    document.addEventListener('keydown', event => { if (event.key === 'Escape') closeDrawers(); });

    $$('[data-filter-target]').forEach(input => input.addEventListener('input', () => {
        const target = document.getElementById(input.dataset.filterTarget);
        const query = input.value.toLowerCase();
        $$('.filterable', target).forEach(row => row.hidden = !row.textContent.toLowerCase().includes(query));
    }));
    $$('[data-status-filter]').forEach(select => select.addEventListener('change', () => {
        const target = document.getElementById(select.dataset.statusFilter);
        const value = select.value.toLowerCase();
        $$('.filterable', target).forEach(row => row.hidden = value !== 'all' && !(row.dataset.status || '').toLowerCase().includes(value));
    }));
    $$('[data-tabs]').forEach(tabset => {
        const tabs = $$('.tab', tabset);
        tabs.forEach(tab => tab.addEventListener('click', () => {
            tabs.forEach(item => item.classList.remove('is-active'));
            tab.classList.add('is-active');
            const value = tab.dataset.tab;
            const targetId = tabset.dataset.tabs;
            $$('.filterable', document.getElementById(targetId)).forEach(row => row.hidden = value !== 'all' && row.dataset.status !== value);
        }));
    });
    $$('[data-steps]').forEach(stepper => {
        const panels = $$('.step-panel', stepper);
        $$('.step', stepper).forEach(step => step.addEventListener('click', () => {
            $$('.step', stepper).forEach(item => item.classList.remove('is-active'));
            step.classList.add('is-active');
            panels.forEach(panel => panel.classList.toggle('is-active', panel.dataset.panel === step.dataset.step));
        }));
    });
    $$('[data-switch]').forEach(toggle => toggle.addEventListener('click', () => toggle.classList.toggle('is-on')));
    $$('[data-demo-submit]').forEach(form => form.addEventListener('submit', event => {
        event.preventDefault();
        showToast('Prototype only — no changes have been saved.');
    }));
})();
