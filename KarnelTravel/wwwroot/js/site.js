// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring and using static web assets.

(function () {
  'use strict';

  if (window.__karnelCursor || !window.matchMedia) return;

  var finePointer = window.matchMedia('(hover: hover) and (pointer: fine)');
  var reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
  if (!finePointer.matches || reducedMotion.matches) return;

  var root = document.getElementById('klc');
  if (!root) return;

  var dot = root.querySelector('[data-klc="dot"]');
  var halo = root.querySelector('[data-klc="halo"]');
  var label = root.querySelector('[data-klc="label"]');
  if (!dot || !halo || !label) return;

  var target = { x: 0, y: 0 };
  var pointer = { x: 0, y: 0 };
  var haloPosition = { x: 0, y: 0 };
  var state = 'default';
  var pressed = false;
  var visible = false;
  var disposed = false;
  var rafId = 0;
  var labels = { image: 'Explore', view: 'View', discover: 'Discover', route: 'View route', drag: 'Drag' };

  function classify(element) {
    if (!element || !(element instanceof Element)) return 'default';
    if (element.closest('[disabled], [aria-disabled="true"]')) return 'disabled';

    var explicit = element.closest('[data-cursor]');
    if (explicit) {
      var requested = explicit.getAttribute('data-cursor');
      if (requested === 'explore' || requested === 'image') return 'image';
      if (requested === 'view' || requested === 'discover' || requested === 'route' || requested === 'drag') return requested;
    }

    if (element.closest('input:not([type="button"]):not([type="submit"]):not([type="checkbox"]):not([type="radio"]):not([type="reset"]):not([type="file"]):not([type="range"]):not([type="color"]), textarea, [contenteditable="true"], [contenteditable=""]')) return 'text';
    if (element.closest('button, a.btn, [role="button"], input[type="button"], input[type="submit"], input[type="reset"]')) return 'button';
    if (element.closest('img, picture, video, canvas, figure.media')) return 'image';
    if (element.closest('a[href], summary, label, [role="link"]')) return 'link';
    return 'default';
  }

  function setState(next) {
    if (next === state) return;
    state = next;
    root.classList.remove('klc--link', 'klc--button', 'klc--image', 'klc--view', 'klc--discover', 'klc--route', 'klc--drag', 'klc--disabled');
    if (state !== 'text') root.classList.add('klc--' + state);
    label.textContent = labels[state] || '';
    root.style.opacity = state === 'text' ? '0' : '';
  }

  function tick() {
    if (disposed) return;
    pointer.x += (target.x - pointer.x) * .9;
    pointer.y += (target.y - pointer.y) * .9;
    haloPosition.x += (pointer.x - haloPosition.x) * .18;
    haloPosition.y += (pointer.y - haloPosition.y) * .18;
    root.style.transform = 'translate3d(' + pointer.x + 'px,' + pointer.y + 'px,0)';
    dot.style.transform = 'translate(-50%,-50%)' + (pressed ? ' scale(.7)' : '');
    halo.style.left = (haloPosition.x - pointer.x) + 'px';
    halo.style.top = (haloPosition.y - pointer.y) + 'px';
    rafId = window.requestAnimationFrame(tick);
  }

  function onMove(event) {
    target.x = event.clientX;
    target.y = event.clientY;
    if (!visible) {
      visible = true;
      root.style.opacity = '';
    }
  }

  function onOver(event) { setState(classify(event.target)); }
  function onOut(event) { setState(classify(event.relatedTarget)); }
  function onDown(event) {
    if (event.button !== 0) return;
    pressed = true;
    root.classList.add('klc--press');
  }
  function onUp() {
    pressed = false;
    root.classList.remove('klc--press');
  }
  function onLeave(event) {
    if (event.relatedTarget === null) {
      visible = false;
      root.style.opacity = '0';
    }
  }

  function dispose() {
    if (disposed) return;
    disposed = true;
    window.cancelAnimationFrame(rafId);
    window.removeEventListener('mousemove', onMove);
    document.removeEventListener('mouseover', onOver, true);
    document.removeEventListener('mouseout', onOut, true);
    document.removeEventListener('mousedown', onDown, true);
    document.removeEventListener('mouseup', onUp, true);
    document.removeEventListener('mouseleave', onLeave, true);
    document.documentElement.classList.remove('klc-active');
    if (root.parentNode) root.parentNode.removeChild(root);
    window.__karnelCursor = null;
  }

  document.documentElement.classList.add('klc-active');
  window.addEventListener('mousemove', onMove, { passive: true });
  document.addEventListener('mouseover', onOver, true);
  document.addEventListener('mouseout', onOut, true);
  document.addEventListener('mousedown', onDown, true);
  document.addEventListener('mouseup', onUp, true);
  document.addEventListener('mouseleave', onLeave, true);
  rafId = window.requestAnimationFrame(tick);
  window.__karnelCursor = { dispose: dispose };
})();
