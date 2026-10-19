export function initializeOutsideClickHandler(element, dotNetReference) {
    function handleClickOutside(event) {
        if (element && !element.contains(event.target)) {
            dotNetReference.invokeMethodAsync('OnOutsideClick');
        }
    }

    function handleKeyDown(event) {
        if (event.key === 'Escape') {
            dotNetReference.invokeMethodAsync('OnOutsideClick');
        }
    }

    document.addEventListener('click', handleClickOutside);
    document.addEventListener('keydown', handleKeyDown);

    window._topbarOutsideClickHandler = { handleClickOutside, handleKeyDown };
}

export function disposeOutsideClickHandler() {
    if (window._topbarOutsideClickHandler) {
        document.removeEventListener('click', window._topbarOutsideClickHandler.handleClickOutside);
        document.removeEventListener('keydown', window._topbarOutsideClickHandler.handleKeyDown);
        delete window._topbarOutsideClickHandler;
    }
}

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