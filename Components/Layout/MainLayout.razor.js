export function initializeDrawerEscapeHandler(dotNetReference) {
    function handleKeyDown(event) {
        if (event.key === 'Escape') {
            dotNetReference.invokeMethodAsync('OnDrawerEscape');
        }
    }

    document.addEventListener('keydown', handleKeyDown);
    window._drawerEscapeHandler = handleKeyDown;
}

export function disposeDrawerEscapeHandler() {
    if (window._drawerEscapeHandler) {
        document.removeEventListener('keydown', window._drawerEscapeHandler);
        delete window._drawerEscapeHandler;
    }
}